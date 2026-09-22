using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Quality.Normalization;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality;

public class RepairApplicationService
{
    private readonly RepairRepository _repairRepository;
    private readonly WeldRepository _weldRepository;
    private readonly IWeldService _weldService;
    private readonly RepairWorkflowService _workflowService;
    private readonly WeldWorkflowEngine _weldWorkflowEngine;
    private readonly IHistoryTrackingService _historyTrackingService;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
    private readonly ProjectRepository _projectRepository;
    private readonly WpsRepository _wpsRepository;

    public RepairApplicationService(        IWeldService weldService,
        WeldRepository weldRepository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        IProjectAccessAuthorizationService projectAccessAuthorization,
        ProjectRepository projectRepository,
        WpsRepository wpsRepository)
    {
        _weldService = weldService;
        _weldRepository = weldRepository;

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _permissionAuthorization = permissionAuthorization
            ?? throw new ArgumentNullException(nameof(permissionAuthorization));

        _projectAccessAuthorization = projectAccessAuthorization
            ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));

        _projectRepository = projectRepository
            ?? throw new ArgumentNullException(nameof(projectRepository));

        _wpsRepository = wpsRepository
            ?? throw new ArgumentNullException(nameof(wpsRepository));

        _repairRepository =
            new RepairRepository(DatabasePath.GetConnectionString());

        _workflowService =
            new RepairWorkflowService();

        _weldWorkflowEngine =
            new WeldWorkflowEngine();

        _historyTrackingService =
            new HistoryTrackingService(
                DatabasePath.GetConnectionString());
    }

    private async Task EnsureRepairAccessAsync(
        RepairRecord repair,
        string permissionKey)
    {
        if (repair == null)
            throw new ArgumentNullException(nameof(repair));

        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                permissionKey);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{permissionKey}'.");
        }

        if (repair.WeldId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The repair is not linked to a valid weld.");
        }

        var weld =
            await _weldService.GetByIdAsync(repair.WeldId);

        if (weld == null)
        {
            throw new UnauthorizedAccessException(
                "The weld linked to this repair could not be found.");
        }

        var project =
            _projectRepository.GetById(weld.ProjectId);

        if (project == null)
        {
            throw new UnauthorizedAccessException(
                "The project linked to this repair could not be found.");
        }

        var hasProjectAccess =
            await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project);

        if (!hasProjectAccess)
        {
            throw new UnauthorizedAccessException(
                "The requested repair is not accessible.");
        }
    }

    public RepairRecord? GetByNcr(Guid ncrId)
    {
        return _repairRepository
            .GetByNcr(ncrId)
            .OrderByDescending(x => x.RepairNumber)
            .FirstOrDefault();
    }

    public RepairRecord EnsureRepairForNcr(
        NcrRecord ncr,
        string requestedBy)
    {
        if (ncr == null)
        {
            throw new ArgumentNullException(
                nameof(ncr));
        }

        if (!ncr.WeldId.HasValue ||
            ncr.WeldId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The NCR is not linked to a weld. " +
                "A repair record cannot be created.");
        }

        if (ncr.DispositionType !=
                NcrDispositionType.Repair &&
            ncr.DispositionType !=
                NcrDispositionType.Rework)
        {
            throw new InvalidOperationException(
                "Only Repair or Rework NCR dispositions " +
                "can create a repair record.");
        }

        var existing =
            GetByNcr(ncr.Id);

        if (existing != null)
        {
            return existing;
        }

        var weldRepairs =
            _repairRepository
                .GetByWeld(
                    ncr.WeldId.Value);

        var activeRepair =
            weldRepairs.FirstOrDefault(
                x =>
                    x.Status == RepairStatus.Requested ||
                    x.Status == RepairStatus.Authorized ||
                    x.Status == RepairStatus.ExcavationInProgress ||
                    x.Status == RepairStatus.RepairWeldingInProgress ||
                    x.Status == RepairStatus.PendingReinspection);

        if (activeRepair != null)
        {
            throw new InvalidOperationException(
                "Weld '" + ncr.WeldId.Value +
                "' already has an active repair cycle #" +
                activeRepair.RepairNumber +
                ". Complete or close the existing repair cycle before creating another repair.");
        }

        var nextRepairNumber =
            weldRepairs.Count == 0
                ? 1
                : weldRepairs.Max(
                    x => x.RepairNumber) + 1;

        var reasonParts =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                ncr.NcrNumber))
        {
            reasonParts.Add(
                $"NCR {ncr.NcrNumber.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(
                ncr.Description))
        {
            reasonParts.Add(
                ncr.Description.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(
                     ncr.CustomReason))
        {
            reasonParts.Add(
                ncr.CustomReason.Trim());
        }

        var repair =
            new RepairRecord
            {
                Id = Guid.NewGuid(),
                WeldId = ncr.WeldId.Value,
                NcrId = ncr.Id,
                RepairNumber =
                    nextRepairNumber,
                Reason =
                    reasonParts.Count == 0
                        ? "Repair generated from NCR."
                        : string.Join(
                            " - ",
                            reasonParts),
                RequestedDate =
                    DateTime.UtcNow,
                Status =
                    RepairStatus.Requested,
                Notes =
                    string.IsNullOrWhiteSpace(
                        requestedBy)
                        ? "Generated from NCR workflow."
                        : $"Generated from NCR workflow by {requestedBy.Trim()}."
            };

        _repairRepository.Add(
            repair);

        return repair;
    }
    public async Task<RepairRecord> CreateNextRepairCycleAsync(
        RepairRecord rejectedRepair,
        string requestedBy)
    {
        await EnsureRepairAccessAsync(
            rejectedRepair,
            PermissionKeys.Quality.Repairs);

        if (rejectedRepair == null)
        {
            throw new ArgumentNullException(
                nameof(rejectedRepair));
        }

        if (rejectedRepair.Status !=
            RepairStatus.Rejected)
        {
            throw new InvalidOperationException(
                "A new repair cycle can only be created " +
                "from a rejected repair.");
        }

        if (rejectedRepair.WeldId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The rejected repair is not linked to a weld.");
        }

        await using var connection =
            new SqliteConnection(
                DatabasePath.GetConnectionString());

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction(
                deferred: false);

        try
        {
            // READ CURRENT WELD INSIDE SAME TRANSACTION

            var weld =
                await _weldRepository.GetByIdAsync(
                    rejectedRepair.WeldId,
                    connection,
                    transaction);

            if (weld == null)
            {
                throw new InvalidOperationException(
                    "The weld linked to the new repair cycle " +
                    "could not be found.");
            }

            // READ REPAIR HISTORY INSIDE SAME TRANSACTION

            var weldRepairs =
                _repairRepository.GetByWeld(
                    rejectedRepair.WeldId,
                    connection,
                    transaction);

            var activeRepair =
                weldRepairs.FirstOrDefault(
                    x =>
                        x.Status == RepairStatus.Requested ||
                        x.Status == RepairStatus.Authorized ||
                        x.Status == RepairStatus.ExcavationInProgress ||
                        x.Status == RepairStatus.RepairWeldingInProgress ||
                        x.Status == RepairStatus.PendingReinspection);

            if (activeRepair != null)
            {
                throw new InvalidOperationException(
                    "Weld '" + rejectedRepair.WeldId +
                    "' already has an active repair cycle #" +
                    activeRepair.RepairNumber +
                    ". Complete or close the existing repair cycle before starting another repair cycle.");
            }

            var nextRepairNumber =
                weldRepairs.Count == 0
                    ? 1
                    : weldRepairs.Max(
                        x => x.RepairNumber) + 1;

            var repair =
                new RepairRecord
                {
                    Id = Guid.NewGuid(),

                    WeldId =
                        rejectedRepair.WeldId,

                    NcrId =
                        rejectedRepair.NcrId,

                    RepairNumber =
                        nextRepairNumber,

                    Reason =
                        $"Repair cycle {nextRepairNumber} " +
                        $"created after rejected repair " +
                        $"#{rejectedRepair.RepairNumber}.",

                    RequestedDate =
                        DateTime.UtcNow,

                    Status =
                        RepairStatus.Requested,

                    Notes =
                        string.IsNullOrWhiteSpace(
                            requestedBy)
                            ? $"Created from rejected repair #{rejectedRepair.RepairNumber}."
                            : $"Created from rejected repair #{rejectedRepair.RepairNumber} by {requestedBy.Trim()}"
                };

            // RESTORE WELD TO REPAIR-REQUIRED

            if (weld.WorkflowStatus !=
                WeldWorkflowStatus.RepairRequired)
            {
                var moveToRepair =
                    _weldWorkflowEngine.MoveToRepair(
                        weld,
                        out var workflowError);

                if (!moveToRepair)
                {
                    throw new InvalidOperationException(
                        "Unable to move weld '" +
                        weld.WeldNumber +
                        "' to RepairRequired for the new repair cycle. " +
                        workflowError);
                }

                await _weldRepository.UpdateWorkflowAsync(
                    weld,
                    connection,
                    transaction);
            }

            // INSERT NEW REPAIR IN SAME TRANSACTION

            _repairRepository.Add(
                repair,
                connection,
                transaction);

            // COMMIT ONLY AFTER BOTH OPERATIONS SUCCEED

            transaction.Commit();

            return repair;
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original exception.
            }

            throw;
        }
    }
    public async Task<List<RepairRecord>> GetProjectRepairs(Guid projectId)
    {
        var repairs = new List<RepairRecord>();

        var welds =
            await _weldService.GetByProjectAsync(projectId);

        foreach (var weld in welds)
        {
            repairs.AddRange(
                _repairRepository.GetByWeld(weld.Id));
        }

        return repairs;
    }

    public async Task<RepairAnalytics> GetAnalytics(Guid projectId)
    {
        var repairs = await GetProjectRepairs(projectId);

        var analyticsService = new RepairAnalyticsService();

        return analyticsService.Generate(repairs);
    }

    public async Task<List<RepairRecord>> GetEnterpriseRepairs()
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authentication is required.");

        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            throw new UnauthorizedAccessException(
                "A valid user identity is required.");

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.Repairs);

        if (!allowed)
            throw new UnauthorizedAccessException(
                "You do not have permission to view repair records.");

        var projects =
            _projectRepository.GetAll().ToList();

        var repairs = new List<RepairRecord>();

        foreach (var project in projects)
        {
            if (await _projectAccessAuthorization
                .CanAccessProjectAsync(
                    _currentUser.UserId,
                    project))
            {
                repairs.AddRange(
                    await GetProjectRepairs(project.Id));
            }
        }

        return repairs;
    }
    public async Task<RepairAnalytics> GetEnterpriseAnalytics()
    {
        var repairs =
            await GetEnterpriseRepairs();

        var analyticsService =
            new RepairAnalyticsService();

        return analyticsService.Generate(repairs);
    }

    /// <summary>
    /// Returns only WPS revisions that are applicable to the selected repair:
    /// the project's welding/fabrication subcontractor, approved, active, and matching the weld material group.
    /// </summary>
    public async Task<List<Wps>> GetApplicableRepairWpsAsync(
        Guid repairId)
    {
        var repair = _repairRepository.GetAll()
            .FirstOrDefault(x => x.Id == repairId);

        if (repair == null)
            throw new InvalidOperationException("Repair record could not be found.");

        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        var weld = await _weldService.GetByIdAsync(repair.WeldId);
        if (weld == null)
            throw new InvalidOperationException("The weld linked to this repair could not be found.");

        var project = _projectRepository.GetById(weld.ProjectId);
        if (project == null)
            return new List<Wps>();

        // W18.2:
        // Prefer the assigned welding/fabrication subcontractor.
        // If no subcontractor is assigned, use the project company.
        var weldingCompanyId =
            project.WeldingSubcontractorCompanyId
            ?? project.CompanyId;

        if (!weldingCompanyId.HasValue ||
            weldingCompanyId.Value == Guid.Empty)
            return new List<Wps>();

        var resolvedWeldingCompanyId = weldingCompanyId.Value;

        var weldMaterial = ComplianceNormalizer.NormalizeMaterialGroup(
            string.IsNullOrWhiteSpace(weld.MaterialGroup)
                ? weld.MaterialSpecification
                : weld.MaterialGroup);

        if (string.IsNullOrWhiteSpace(weldMaterial))
            return new List<Wps>();

        return _wpsRepository.GetByCompany(resolvedWeldingCompanyId)
            .Where(x => x.IsApproved && x.IsActive)
            .Where(x => ComplianceNormalizer.NormalizeMaterialGroup(x.MaterialGroup) == weldMaterial)
            .OrderBy(x => x.WpsNumber)
            .ThenByDescending(x => x.Revision)
            .ToList();
    }

    // ==================================================
    // REPAIR WORKFLOW OPERATIONS
    // ==================================================

    public async Task<(bool Success, string Error)> AuthorizeRepairAsync(
        RepairRecord repair,
        string authorizedBy)
    {
        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        if (AuthorizeRepair(repair, authorizedBy, out var error))
            return (true, string.Empty);

        return (false, error);
    }

    public async Task<(bool Success, string Error)> StartExcavationAsync(
        RepairRecord repair,
        string excavationMethod)
    {
        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        if (StartExcavation(repair, excavationMethod, out var error))
            return (true, string.Empty);

        return (false, error);
    }

            public async Task<(bool Success, string Error)> CloseRepairAsync(
        RepairRecord repair)
    {
        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        if (CloseRepair(repair, out var error))
            return (true, string.Empty);

        return (false, error);
    }

    public bool AuthorizeRepair(
        RepairRecord repair,
        string authorizedBy,
        out string error)
    {
        if (repair == null)
        {
            error = "Repair record is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(authorizedBy))
        {
            error = "Authorized By is required.";
            return false;
        }

        if (!_workflowService.AuthorizeRepair(
                repair,
                authorizedBy.Trim(),
                out error))
        {
            return false;
        }

        _repairRepository.Update(repair);

        return true;
    }

    public bool StartExcavation(
        RepairRecord repair,
        string excavationMethod,
        out string error)
    {
        if (repair == null)
        {
            error = "Repair record is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(excavationMethod))
        {
            error = "Excavation method is required.";
            return false;
        }

        if (!_workflowService.StartExcavation(
                repair,
                out error))
        {
            return false;
        }

        repair.ExcavationMethod =
            excavationMethod.Trim();

        _repairRepository.Update(repair);

        return true;
    }

    public async Task<(bool Success, string Error)>
        StartRepairWeldingAsync(
        RepairRecord repair,
        string repairedByWelder,
        Guid? repairWpsId)
    {
        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        if (repair == null)
        {
            return (false, "Repair record is required.");
        }

        if (string.IsNullOrWhiteSpace(repairedByWelder))
        {
            return (false, "Repair welder is required.");
        }

        if (!repairWpsId.HasValue || repairWpsId.Value == Guid.Empty)
        {
            return (false, "An approved active repair WPS must be selected.");
        }

        await using var connection =
            new SqliteConnection(DatabasePath.GetConnectionString());

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction(deferred: false);

        try
        {
            var weld = await _weldRepository.GetByIdAsync(
                repair.WeldId,
                connection,
                transaction);

            if (weld == null)
            {
                transaction.Rollback();
                return (false, "The weld linked to this repair could not be found.");
            }

            var project = _projectRepository.GetById(weld.ProjectId);
            if (project == null)
            {
                transaction.Rollback();
                return (false, "The project linked to this repair could not be found.");
            }

            // W18.2:
            // Prefer the assigned welding/fabrication subcontractor.
            // If no subcontractor is assigned, use the project company.
            var weldingCompanyId =
                project.WeldingSubcontractorCompanyId
                ?? project.CompanyId;

            if (!weldingCompanyId.HasValue ||
                weldingCompanyId.Value == Guid.Empty)
            {
                transaction.Rollback();
                return (false, "The project has no responsible company assigned for repair WPS selection.");
            }

            var repairWps = _wpsRepository.GetById(
                repairWpsId.Value,
                connection,
                transaction);

            if (repairWps == null)
            {
                transaction.Rollback();
                return (false, "The selected repair WPS revision could not be found.");
            }

            if (!repairWps.CompanyId.HasValue ||
                repairWps.CompanyId.Value != weldingCompanyId.Value)
            {
                transaction.Rollback();
                return (false, "The selected repair WPS does not belong to the project's welding/fabrication subcontractor.");
            }

            if (!repairWps.IsApproved)
            {
                transaction.Rollback();
                return (false, $"Repair WPS '{repairWps.WpsNumber} Rev {repairWps.Revision}' is not approved and cannot be used for repair welding.");
            }

            if (!repairWps.IsActive)
            {
                transaction.Rollback();
                return (false, $"Repair WPS '{repairWps.WpsNumber} Rev {repairWps.Revision}' is not active and cannot be used for repair welding.");
            }

            var weldMaterial = ComplianceNormalizer.NormalizeMaterialGroup(
                string.IsNullOrWhiteSpace(weld.MaterialGroup)
                    ? weld.MaterialSpecification
                    : weld.MaterialGroup);
            var wpsMaterial = ComplianceNormalizer.NormalizeMaterialGroup(
                repairWps.MaterialGroup);

            if (string.IsNullOrWhiteSpace(weldMaterial) ||
                string.IsNullOrWhiteSpace(wpsMaterial) ||
                weldMaterial != wpsMaterial)
            {
                transaction.Rollback();
                return (false, $"The selected repair WPS material group '{repairWps.MaterialGroup}' does not match the weld material group '{(string.IsNullOrWhiteSpace(weld.MaterialGroup) ? weld.MaterialSpecification : weld.MaterialGroup)}'.");
            }

            if (weld.WorkflowStatus != WeldWorkflowStatus.RepairRequired &&
                weld.WorkflowStatus != WeldWorkflowStatus.UnderRepair)
            {
                transaction.Rollback();
                return (false, $"Repair welding cannot start while weld '{weld.WeldNumber}' is in workflow status '{weld.WorkflowStatus}'.");
            }

            if (!_workflowService.StartRepairWelding(
                    repair,
                    out var repairError))
            {
                transaction.Rollback();
                return (false, repairError);
            }

            if (weld.WorkflowStatus == WeldWorkflowStatus.RepairRequired)
            {
                if (!_weldWorkflowEngine.MarkUnderRepair(
                        weld,
                        out var weldError))
                {
                    transaction.Rollback();
                    return (false, weldError);
                }
            }

            repair.RepairedByWelder = repairedByWelder.Trim();
            repair.RepairWpsId = repairWps.Id;
            repair.RepairWpsNumber = repairWps.WpsNumber.Trim();

            _repairRepository.Update(repair, connection, transaction);

            await _weldRepository.UpdateWorkflowAsync(
                weld,
                connection,
                transaction);

            transaction.Commit();

            return (true, string.Empty);
        }
        catch
        {
            try { transaction.Rollback(); } catch { }
            throw;
        }
    }

    public async Task<(bool Success, string Error)>
        SendForReinspectionAsync(
        RepairRecord repair)
    {
        await EnsureRepairAccessAsync(
            repair,
            PermissionKeys.Quality.Repairs);

        if (repair == null)
        {
            return (
                false,
                "Repair record is required.");
        }

        await using var connection =
            new Microsoft.Data.Sqlite.SqliteConnection(
                DatabasePath.GetConnectionString());

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction(deferred: false);

        try
        {
            var weld =
                await _weldRepository.GetByIdAsync(
                    repair.WeldId,
                    connection,
                    transaction);

            if (weld == null)
            {
                transaction.Rollback();

                return (
                    false,
                    "The weld linked to this repair could not be found.");
            }

            if (weld.WorkflowStatus !=
                WeldWorkflowStatus.UnderRepair)
            {
                transaction.Rollback();

                return (
                    false,
                    $"Reinspection cannot be requested while weld " +
                    $"'{weld.WeldNumber}' is in workflow status " +
                    $"'{weld.WorkflowStatus}'.");
            }

            if (!_workflowService.SendForReinspection(
                    repair,
                    out var repairError))
            {
                transaction.Rollback();

                return (
                    false,
                    repairError);
            }

            if (!_weldWorkflowEngine.MarkReinspectionRequired(
                    weld,
                    out var reinspectionError))
            {
                transaction.Rollback();

                return (
                    false,
                    reinspectionError);
            }

            var ndtPendingTransition =
                _weldWorkflowEngine.TryTransition(
                    weld,
                    WeldWorkflowStatus.NdtPending);

            if (!ndtPendingTransition.Success)
            {
                transaction.Rollback();

                return (
                    false,
                    ndtPendingTransition.ErrorMessage);
            }

            _repairRepository.Update(
                repair,
                connection,
                transaction);

            await _weldRepository.UpdateWorkflowAsync(
                weld,
                connection,
                transaction);

            transaction.Commit();

            return (
                true,
                string.Empty);
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
            }

            throw;
        }
    }

            public bool CloseRepair(
        RepairRecord repair,
        out string error)
    {
        if (repair == null)
        {
            error = "Repair record is required.";
            return false;
        }

        if (!_workflowService.CloseRepair(
                repair,
                out error))
        {
            return false;
        }

        if (!repair.CompletedDate.HasValue)
        {
            repair.CompletedDate =
                DateTime.UtcNow;
        }

        _repairRepository.Update(repair);

        return true;
    }
}










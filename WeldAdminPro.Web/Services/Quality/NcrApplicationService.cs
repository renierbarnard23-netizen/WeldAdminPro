using WeldAdminPro.Data;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

using WeldAdminPro.Data.Services.Quality;

namespace WeldAdminPro.Web.Services.Quality;

public class NcrApplicationService
{
    private readonly NcrRepository _repository;

    private readonly NcrWorkflowHistoryRepository
        _historyRepository;

    private readonly RepairApplicationService
        _repairApplicationService;

    private readonly ICurrentUserContext
        _currentUser;

    private readonly IPermissionAuthorizationService
        _permissionAuthorization;

    private readonly IProjectAccessAuthorizationService
        _projectAccessAuthorization;

    private readonly WeldRepository
        _weldRepository;

    private readonly ProjectRepository
        _projectRepository;

    public NcrApplicationService(
        NcrRepository repository,
        NcrWorkflowHistoryRepository historyRepository,
        RepairApplicationService repairApplicationService,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        IProjectAccessAuthorizationService projectAccessAuthorization)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _repairApplicationService =
            repairApplicationService;

        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization
            ?? throw new ArgumentNullException(nameof(permissionAuthorization));
        _projectAccessAuthorization = projectAccessAuthorization
            ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));

        _weldRepository =
            new WeldRepository(
                DatabasePath.GetConnectionString());

        _projectRepository =
            new ProjectRepository();
    }

    // =====================================================
    // Queries
    // =====================================================

    public List<NcrRecord> GetAll()
    {
        return _repository.GetAll();
    }

    public List<NcrRecord> GetByWeld(
        Guid weldId)
    {
        return _repository.GetByWeld(weldId);
    }

    public NcrRecord? GetById(
        Guid id)
    {
        return _repository.GetById(id);
    }

    public List<NcrWorkflowHistoryEntry> GetHistory(
        Guid ncrId)
    {
        return _historyRepository.GetByNcr(ncrId);
    }

    // =====================================================
    // Commands
    // =====================================================

    public async Task Create(
        NcrRecord ncr)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.NCR);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.NCR}'.");
        }



        await EnsureNcrProjectAccessAsync(ncr);

        if (ncr.Id == Guid.Empty)
        {
            ncr.Id = Guid.NewGuid();
        }

        if (ncr.RaisedDate == default)
        {
            ncr.RaisedDate = DateTime.Now;
        }

        ncr.Status = NcrStatus.Open;
        ncr.IsClosed = false;
        ncr.ClosedBy = string.Empty;
        ncr.ClosedDate = null;

        _repository.Add(ncr);

        AddHistory(
            ncr.Id,
            null,
            ncr.Status,
            "Created",
            ncr.RaisedBy,
            BuildCreatedDetails(ncr));
    }

    public async Task Update(
        NcrRecord ncr)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.NCR);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.NCR}'.");
        }


        var existingNcr = GetById(ncr.Id);

        if (existingNcr == null)
        {
            return;
        }

        await EnsureNcrProjectAccessAsync(existingNcr);

        _repository.Update(ncr);
    }

    public bool CanMoveTo(
        NcrRecord ncr,
        NcrStatus target)
    {
        return NcrWorkflowService.CanMoveTo(
            ncr.Status,
            target);
    }

    public async Task<bool> MoveTo(
        Guid id,
        NcrStatus target)
    {
        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        await EnsureNcrProjectAccessAsync(ncr);

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                target))
        {
            return false;
        }

        var requiredPermission =
            target switch
            {
                NcrStatus.ApprovedForRepair =>
                    PermissionKeys.Quality.NcrDisposition,

                NcrStatus.Rejected =>
                    PermissionKeys.Quality.NcrDisposition,

                NcrStatus.PendingVerification =>
                    ncr.Status == NcrStatus.RepairInProgress
                        ? PermissionKeys.Quality.Repairs
                        : PermissionKeys.Quality.NcrDisposition,

                NcrStatus.RepairInProgress =>
                    PermissionKeys.Quality.Repairs,

                NcrStatus.Closed =>
                    PermissionKeys.Quality.NcrClose,

                NcrStatus.UnderInvestigation =>
                    PermissionKeys.Quality.NCR,

                NcrStatus.AwaitingDisposition =>
                    PermissionKeys.Quality.NCR,

                _ =>
                    PermissionKeys.Quality.NCR
            };

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                requiredPermission);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{requiredPermission}'.");
        }


        var previousStatus =
            ncr.Status;

        ncr.Status = target;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            target,
            "Status Changed",
            ResolveActor(ncr),
            $"Status changed from {previousStatus} to {target}.");

        return true;
    }

    public async Task<bool> StartRepairExecution(
        Guid id,
        string performedBy)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.Repairs);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.Repairs}'.");
        }

        if (string.IsNullOrWhiteSpace(performedBy))
        {
            return false;
        }

        var ncr = GetById(id);

        if (ncr == null ||
            ncr.Status != NcrStatus.ApprovedForRepair)
        {
            return false;
        }

        await EnsureNcrProjectAccessAsync(ncr);

        const NcrStatus target =
            NcrStatus.RepairInProgress;

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                target))
        {
            return false;
        }
        // Create or reuse the repair linked to this NCR
        // before moving the NCR into RepairInProgress.
        _repairApplicationService
            .EnsureRepairForNcr(
                ncr,
                performedBy.Trim());

        var previousStatus =
            ncr.Status;

        ncr.Status =
            target;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            target,
            "Repair / Rework Started",
            performedBy.Trim(),
            $"Repair / rework execution started by {performedBy.Trim()}.");

        return true;
    }


    public async Task<bool> CompleteRepairExecutionAsync(
        Guid id,
        string performedBy)
    {
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
                PermissionKeys.Quality.Repairs);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.Repairs}'.");
        }

        var ncr = GetById(id);

        if (ncr == null ||
            !ncr.WeldId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "The requested NCR is not accessible.");
        }

        var weld =
            await _weldRepository.GetByIdAsync(ncr.WeldId.Value);

        if (weld == null)
        {
            throw new UnauthorizedAccessException(
                "The NCR's weld could not be found.");
        }

        var project =
            _projectRepository.GetById(weld.ProjectId);

        if (project == null)
        {
            throw new UnauthorizedAccessException(
                "The NCR's project could not be found.");
        }

        var hasProjectAccess =
            await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project);

        if (!hasProjectAccess)
        {
            throw new UnauthorizedAccessException(
                "The requested NCR is not accessible.");
        }

        return CompleteRepairExecution(
            id,
            performedBy);
    }

    private async Task EnsureNcrProjectAccessAsync(NcrRecord ncr)
    {
        if (!ncr.WeldId.HasValue)
        {
            return;
        }
        var weld =
            await _weldRepository.GetByIdAsync(ncr.WeldId.Value);

        if (weld == null)
        {
            throw new UnauthorizedAccessException(
                "The NCR's weld could not be found.");
        }

        var project =
            _projectRepository.GetById(weld.ProjectId);

        if (project == null)
        {
            throw new UnauthorizedAccessException(
                "The NCR's project could not be found.");
        }

        var hasProjectAccess =
            await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project);

        if (!hasProjectAccess)
        {
            throw new UnauthorizedAccessException(
                "The requested NCR is not accessible.");
        }
    }
    private bool CompleteRepairExecution(
        Guid id,
        string performedBy)
    {
        if (string.IsNullOrWhiteSpace(performedBy))
        {
            return false;
        }

        var ncr = GetById(id);

        if (ncr == null ||
            ncr.Status != NcrStatus.RepairInProgress)
        {
            return false;
        }


        // The NCR may only leave RepairInProgress after
        // its linked controlled repair lifecycle is closed.
        var linkedRepair =
            _repairApplicationService
                .GetByNcr(ncr.Id);

        if (linkedRepair == null ||
            linkedRepair.Status != RepairStatus.Closed)
        {
            return false;
        }

        const NcrStatus target =
            NcrStatus.PendingVerification;

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                target))
        {
            return false;
        }

        var previousStatus =
            ncr.Status;

        ncr.Status =
            target;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            target,
            "Repair / Rework Completed",
            performedBy.Trim(),
            $"Repair / rework execution completed by {performedBy.Trim()}.");

        return true;
    }

    public async Task<bool> SetDisposition(
        Guid id,
        NcrDispositionType disposition,
        string approvedBy,
        bool requiresCustomerApproval = false,
        bool customerApproved = false,
        string? customerApprovalReference = null)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.NcrDisposition);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.NcrDisposition}'.");
        }

        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        await EnsureNcrProjectAccessAsync(ncr);

        if (ncr.Status !=
            NcrStatus.AwaitingDisposition)
        {
            return false;
        }

        ncr.DispositionType = disposition;
        ncr.DispositionApprovedBy = approvedBy;
        ncr.DispositionApprovedDate = DateTime.Now;
        ncr.RequiresCustomerApproval =
            requiresCustomerApproval;
        ncr.CustomerApproved =
            customerApproved;
        ncr.CustomerApprovalReference =
            customerApprovalReference;

        _repository.Update(ncr);

        var details =
            $"Disposition: {disposition}. " +
            $"Customer approval required: " +
            $"{(requiresCustomerApproval ? "Yes" : "No")}.";

        if (requiresCustomerApproval)
        {
            details +=
                $" Customer approved: " +
                $"{(customerApproved ? "Yes" : "No")}.";

            if (!string.IsNullOrWhiteSpace(
                    customerApprovalReference))
            {
                details +=
                    $" Approval reference: " +
                    $"{customerApprovalReference}.";
            }
        }

        AddHistory(
            ncr.Id,
            ncr.Status,
            ncr.Status,
            "Disposition Recorded",
            approvedBy,
            details);

        // Customer approval is a workflow gate.
        // Record the disposition, but do not advance
        // until the required approval has been obtained.
        if (requiresCustomerApproval &&
            !customerApproved)
        {
            return true;
        }

        var target =
            NcrWorkflowService
                .GetPostDispositionStatus(
                    disposition);

        // Engineering Review intentionally remains
        // AwaitingDisposition until engineering reaches
        // a final disposition decision.
        if (!target.HasValue)
        {
            return true;
        }

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                target.Value))
        {
            return false;
        }

        var previousStatus =
            ncr.Status;

        ncr.Status =
            target.Value;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            target.Value,
            "Disposition Workflow Advanced",
            approvedBy,
            $"Disposition {disposition} advanced " +
            $"the NCR from {previousStatus} " +
            $"to {target.Value}.");

        return true;
    }

    public async Task<bool> RecordVerification(
        Guid id,
        string verifiedBy)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.NcrVerify);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.NcrVerify}'.");
        }

        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        await EnsureNcrProjectAccessAsync(ncr);

        if (ncr.Status != NcrStatus.PendingVerification)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(verifiedBy))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                ncr.VerificationBy) ||
            ncr.VerificationDate.HasValue)
        {
            return false;
        }

        var actor =
            verifiedBy.Trim();

        ncr.VerificationBy = actor;
        ncr.VerificationDate = DateTime.Now;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            ncr.Status,
            ncr.Status,
            "Verification Recorded",
            actor,
            $"Verification completed by {actor}.");

        return true;
    }

    public async Task<bool> Close(
        Guid id,
        string closedBy)
    {
        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.NcrClose);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to perform '{PermissionKeys.Quality.NcrClose}'.");
        }

        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        await EnsureNcrProjectAccessAsync(ncr);

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                NcrStatus.Closed))
        {
            return false;
        }

        if (ncr.RequiresCustomerApproval &&
            !ncr.CustomerApproved)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
                ncr.VerificationBy) ||
            !ncr.VerificationDate.HasValue)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(closedBy))
        {
            return false;
        }

        closedBy =
            closedBy.Trim();

        var previousStatus =
            ncr.Status;

        ncr.Status = NcrStatus.Closed;
        ncr.IsClosed = true;
        ncr.ClosedBy = closedBy;
        ncr.ClosedDate = DateTime.Now;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            NcrStatus.Closed,
            "Closed",
            closedBy,
            $"NCR closed by {closedBy}.");

        return true;
    }

    public string GetNextNcrNumber()
    {
        return _repository.GetNextNcrNumber();
    }

    // =====================================================
    // History
    // =====================================================

    private void AddHistory(
        Guid ncrId,
        NcrStatus? fromStatus,
        NcrStatus toStatus,
        string action,
        string? performedBy,
        string? details)
    {
        _historyRepository.Add(
            new NcrWorkflowHistoryEntry
            {
                Id = Guid.NewGuid(),
                NcrId = ncrId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Action = action,
                PerformedBy =
                    performedBy ?? string.Empty,
                PerformedDate = DateTime.Now,
                Details =
                    details ?? string.Empty
            });
    }

    private static string ResolveActor(
        NcrRecord ncr)
    {
        if (!string.IsNullOrWhiteSpace(
                ncr.AssignedTo))
        {
            return ncr.AssignedTo;
        }

        if (!string.IsNullOrWhiteSpace(
                ncr.RaisedBy))
        {
            return ncr.RaisedBy;
        }

        return "System";
    }

    private static string BuildCreatedDetails(
        NcrRecord ncr)
    {
        var category =
            string.IsNullOrWhiteSpace(ncr.Category)
                ? "Not specified"
                : ncr.Category;

        var details =
            $"NCR {ncr.NcrNumber} created. " +
            $"Category: {category}. " +
            $"Welding related: " +
            $"{(ncr.IsWeldingRelated ? "Yes" : "No")}.";

        if (!string.IsNullOrWhiteSpace(
                ncr.CustomReason))
        {
            details +=
                $" Custom reason: {ncr.CustomReason}.";
        }

        if (ncr.WeldId.HasValue)
        {
            details +=
                $" Associated weld: {ncr.WeldNumber}.";
        }

        return details;
    }
}



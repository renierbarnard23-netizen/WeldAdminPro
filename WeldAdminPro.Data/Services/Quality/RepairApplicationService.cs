using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality;

public class RepairApplicationService
{
    private readonly RepairRepository _repairRepository;
    private readonly IWeldService _weldService;
    private readonly RepairWorkflowService _workflowService;
    private readonly WeldWorkflowEngine _weldWorkflowEngine;
    private readonly IHistoryTrackingService _historyTrackingService;

    public RepairApplicationService(IWeldService weldService)
    {
        _weldService = weldService;

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

    public RepairRecord? GetByNcr(Guid ncrId)
    {
        return _repairRepository
            .GetByNcr(ncrId)
            .OrderBy(x => x.RepairNumber)
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

    public Task<List<RepairRecord>> GetEnterpriseRepairs()
    {
        return Task.FromResult(
            _repairRepository.GetAll());
    }
    public Task<RepairAnalytics> GetEnterpriseAnalytics()
    {
        var repairs = _repairRepository.GetAll();

        var analyticsService = new RepairAnalyticsService();

        return Task.FromResult(
            analyticsService.Generate(repairs));
    }

    // ==================================================
    // REPAIR WORKFLOW OPERATIONS
    // ==================================================

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
        string repairWpsNumber)
    {
        if (repair == null)
        {
            return (
                false,
                "Repair record is required.");
        }

        if (string.IsNullOrWhiteSpace(repairedByWelder))
        {
            return (
                false,
                "Repair welder is required.");
        }

        if (string.IsNullOrWhiteSpace(repairWpsNumber))
        {
            return (
                false,
                "Repair WPS number is required.");
        }

        var weld =
            await _weldService.GetByIdAsync(
                repair.WeldId);

        if (weld == null)
        {
            return (
                false,
                "The weld linked to this repair could not be found.");
        }

        if (weld.WorkflowStatus !=
            WeldWorkflowStatus.RepairRequired)
        {
            return (
                false,
                $"Repair welding cannot start while weld " +
                $"'{weld.WeldNumber}' is in workflow status " +
                $"'{weld.WorkflowStatus}'.");
        }

        if (!_workflowService.StartRepairWelding(
                repair,
                out var repairError))
        {
            return (
                false,
                repairError);
        }

        var weldTransition =
            _weldWorkflowEngine.MarkUnderRepair(
                weld,
                out var weldError);

        if (!weldTransition)
        {
            return (
                false,
                weldError);
        }

        repair.RepairedByWelder =
            repairedByWelder.Trim();

        repair.RepairWpsNumber =
            repairWpsNumber.Trim();

        _repairRepository.Update(
            repair);

        await _weldService.UpdateAsync(
            weld);

        return (
            true,
            string.Empty);
    }

    public async Task<(bool Success, string Error)>
        SendForReinspectionAsync(
        RepairRecord repair)
    {
        if (repair == null)
        {
            return (
                false,
                "Repair record is required.");
        }

        var weld =
            await _weldService.GetByIdAsync(
                repair.WeldId);

        if (weld == null)
        {
            return (
                false,
                "The weld linked to this repair could not be found.");
        }

        if (weld.WorkflowStatus !=
            WeldWorkflowStatus.UnderRepair)
        {
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
            return (
                false,
                repairError);
        }

        if (!_weldWorkflowEngine.MarkReinspectionRequired(
                weld,
                out var reinspectionError))
        {
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
            return (
                false,
                ndtPendingTransition.ErrorMessage);
        }

        _repairRepository.Update(
            repair);

        await _weldService.UpdateAsync(
            weld);

        return (
            true,
            string.Empty);
    }

    public bool AcceptRepair(
        RepairRecord repair,
        string reinspectionResult,
        out string error)
    {
        if (repair == null)
        {
            error = "Repair record is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(reinspectionResult))
        {
            error = "Reinspection result is required.";
            return false;
        }

        if (!_workflowService.AcceptRepair(
                repair,
                out error))
        {
            return false;
        }

        repair.ReinspectionResult =
            reinspectionResult.Trim();

        repair.CompletedDate =
            DateTime.UtcNow;

        _repairRepository.Update(repair);

        return true;
    }

    public bool RejectRepair(
        RepairRecord repair,
        string reinspectionResult,
        out string error)
    {
        if (repair == null)
        {
            error = "Repair record is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(reinspectionResult))
        {
            error = "Reinspection result is required.";
            return false;
        }

        if (!_workflowService.RejectRepair(
                repair,
                out error))
        {
            return false;
        }

        repair.ReinspectionResult =
            reinspectionResult.Trim();

        _repairRepository.Update(repair);

        return true;
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

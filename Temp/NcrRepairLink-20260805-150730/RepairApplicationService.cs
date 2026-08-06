using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
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
}

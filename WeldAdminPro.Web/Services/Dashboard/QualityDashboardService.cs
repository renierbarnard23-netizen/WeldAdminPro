using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Web.Services.Quality;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityDashboardService
{
    public async Task<QualitySnapshot> GetDashboard()
    {

        var compliance = await _complianceService.CalculateAsync();        

        var snapshot = new QualitySnapshot
        {
            ComplianceScore = compliance.Score,
            ComplianceRating = compliance.Rating,
            ComplianceSummary = compliance.Summary,

            ActivePqr = _pqrService.GetActiveApprovedCount(),
            QualifiedWelders = _welderQualificationService.GetValidQualifiedWelderCount(),
            PendingNdt = _ndtService.GetPendingCount(),
            MissingDocuments = _documentService.GetMissingDocumentCount(),
            ExpiringQualifications = _welderQualificationService.GetExpiringQualificationCount(),
        };

        try
        {
            snapshot.ActiveWps =
                _wpsRepository.GetActive().Count;
        }
        catch
        {
            snapshot.ActiveWps = 0;
        }

        var repairAnalytics =
            await _repairService.GetEnterpriseAnalytics();

        snapshot.OpenRepairs =
            repairAnalytics.OpenRepairs;

        // Build quality alerts
        var alertSummary = _alertService.Build(snapshot);

        snapshot.Alerts = alertSummary.Alerts
            .Select(a => a.Message)
            .ToList();

        // Build AI recommendations
        snapshot.Recommendations =
            _recommendationService.Build(snapshot);

        // Build recent activity
        snapshot.RecentActivity =
            _activityService.Build(snapshot);

        return snapshot;
    }

    private readonly WpsRepository _wpsRepository;
    private readonly PqrApplicationService _pqrService;
    private readonly RepairApplicationService _repairService;
    private readonly WeldRegisterApplicationService _weldService;
    private readonly WelderQualificationApplicationService _welderQualificationService;
    private readonly NdtApplicationService _ndtService;
    private readonly DocumentApplicationService _documentService;
    private readonly QualityComplianceService _complianceService;
    private readonly QualityAlertService _alertService;
    private readonly QualityRecommendationService _recommendationService;
    private readonly QualityActivityService _activityService;

    public QualityDashboardService(
        WpsRepository wpsRepository,
        PqrApplicationService pqrService,
        RepairApplicationService repairService,
        WeldRegisterApplicationService weldService,
        WelderQualificationApplicationService welderQualificationService,
        NdtApplicationService ndtService,
        DocumentApplicationService documentService,
        QualityComplianceService complianceService,
        QualityAlertService alertService,
        QualityRecommendationService recommendationService,
        QualityActivityService activityService)
    {
        _wpsRepository = wpsRepository;
        _pqrService = pqrService;
        _repairService = repairService;
        _weldService = weldService;
        _welderQualificationService = welderQualificationService;
        _ndtService = ndtService;
        _documentService = documentService;
        _complianceService = complianceService;
        _alertService = alertService;
        _recommendationService = recommendationService;
        _activityService = activityService;
    }

}

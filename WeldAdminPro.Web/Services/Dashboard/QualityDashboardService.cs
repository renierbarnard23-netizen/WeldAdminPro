using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Security.Catalog;

using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Web.Services.Quality;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityDashboardService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;

    private readonly WpsApplicationService _wpsService;
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
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        WpsApplicationService wpsService,
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
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;

        _wpsService = wpsService;
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

    public async Task<QualitySnapshot> GetDashboard()
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("Authentication is required.");

        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Authenticated user ID is required.");

        var compliance = await _complianceService.CalculateAsync();

        var snapshot = new QualitySnapshot
        {
            ComplianceScore = compliance.Score,
            ComplianceRating = compliance.Rating,
            ComplianceSummary = compliance.Summary,
            IsComplete = compliance.IsComplete,

            ActiveWpsAvailable = await HasPermissionAsync(PermissionKeys.Quality.WPS),
            ActivePqrAvailable = await HasPermissionAsync(PermissionKeys.Quality.PQR),
            QualifiedWeldersAvailable = await HasPermissionAsync(PermissionKeys.Quality.WelderQualifications),
            PendingNdtAvailable = await HasPermissionAsync(PermissionKeys.Quality.NDT),
            OpenRepairsAvailable = await HasPermissionAsync(PermissionKeys.Quality.Repairs),
            MissingDocumentsAvailable = await HasPermissionAsync(PermissionKeys.Quality.View),
            ExpiringQualificationsAvailable = await HasPermissionAsync(PermissionKeys.Quality.WelderQualifications)
        };

        if (snapshot.ActivePqrAvailable)
        {
            snapshot.ActivePqr =
                await _pqrService.GetActiveApprovedCountAsync();
        }

        if (snapshot.QualifiedWeldersAvailable)
        {
            snapshot.QualifiedWelders =
                await _welderQualificationService.GetValidQualifiedWelderCountAsync();
        }

        if (snapshot.PendingNdtAvailable)
        {
            snapshot.PendingNdt =
                await _ndtService.GetPendingCountAsync();
        }

        if (snapshot.MissingDocumentsAvailable)
        {
            snapshot.MissingDocuments =
                await _documentService.GetMissingDocumentCountAsync();
        }

        if (snapshot.ExpiringQualificationsAvailable)
        {
            snapshot.ExpiringQualifications =
                await _welderQualificationService.GetExpiringQualificationCountAsync();
        }

        if (snapshot.ActiveWpsAvailable)
        {
            snapshot.ActiveWps =
                (await _wpsService.GetActiveAsync()).Count;
        }

        if (snapshot.OpenRepairsAvailable)
        {
            var repairAnalytics =
                await _repairService.GetEnterpriseAnalytics();

            snapshot.OpenRepairs =
                repairAnalytics.OpenRepairs;

            snapshot.Repairs =
                await _repairService.GetEnterpriseRepairs();
        }

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

    private async Task<bool> HasPermissionAsync(string permissionKey)
    {
        return await _permissionAuthorization.HasPermissionAsync(
            _currentUser.UserId,
            _currentUser.Role,
            permissionKey);
    }
}

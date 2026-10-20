using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Security.Catalog;
using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Web.Services.Quality;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityComplianceService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;

    private readonly WpsApplicationService _wpsService;
    private readonly PqrApplicationService _pqrService;
    private readonly WelderQualificationApplicationService _welderService;
    private readonly NdtApplicationService _ndtService;
    private readonly DocumentApplicationService _documentService;
    private readonly RepairApplicationService _repairService;

    public QualityComplianceService(
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        WpsApplicationService wpsService,
        PqrApplicationService pqrService,
        WelderQualificationApplicationService welderService,
        NdtApplicationService ndtService,
        DocumentApplicationService documentService,
        RepairApplicationService repairService)
    {
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;

        _wpsService = wpsService;
        _pqrService = pqrService;
        _welderService = welderService;
        _ndtService = ndtService;
        _documentService = documentService;
        _repairService = repairService;
    }

    public async Task<QualityComplianceScore> CalculateAsync()
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("Authentication is required.");

        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Authenticated user ID is required.");

        var score = 0;
        var isComplete = true;

        // WPS
        if (await HasPermissionAsync(PermissionKeys.Quality.WPS))
        {
            if ((await _wpsService.GetActiveAsync()).Count > 0)
                score += 20;
        }
        else
        {
            isComplete = false;
        }

        // PQR
        if (await HasPermissionAsync(PermissionKeys.Quality.PQR))
        {
            if (await _pqrService.GetActiveApprovedCountAsync() > 0)
                score += 20;
        }
        else
        {
            isComplete = false;
        }

        // Welder Qualifications
        if (await HasPermissionAsync(PermissionKeys.Quality.WelderQualifications))
        {
            if (await _welderService.GetValidQualifiedWelderCountAsync() > 0)
                score += 20;
        }
        else
        {
            isComplete = false;
        }

        // Pending NDT
        if (await HasPermissionAsync(PermissionKeys.Quality.NDT))
        {
            var pending = await _ndtService.GetPendingCountAsync();

            score += pending switch
            {
                0 => 15,
                <= 5 => 12,
                <= 10 => 8,
                _ => 0
            };
        }
        else
        {
            isComplete = false;
        }

        // Open Repairs
        if (await HasPermissionAsync(PermissionKeys.Quality.Repairs))
        {
            var repairAnalytics = await _repairService.GetEnterpriseAnalytics();
            var repairs = repairAnalytics.OpenRepairs;

            score += repairs switch
            {
                0 => 10,
                <= 3 => 8,
                <= 10 => 5,
                _ => 0
            };
        }
        else
        {
            isComplete = false;
        }

        // Missing Documents
        if (await HasPermissionAsync(PermissionKeys.Quality.View))
        {
            var missingDocs = await _documentService.GetMissingDocumentCountAsync();

            score += missingDocs switch
            {
                0 => 10,
                <= 5 => 8,
                <= 10 => 5,
                _ => 0
            };
        }
        else
        {
            isComplete = false;
        }

        // Expiring Qualifications
        if (await HasPermissionAsync(PermissionKeys.Quality.WelderQualifications))
        {
            var expiring = await _welderService.GetExpiringQualificationCountAsync();

            score += expiring switch
            {
                0 => 5,
                <= 3 => 4,
                <= 10 => 2,
                _ => 0
            };
        }
        else
        {
            isComplete = false;
        }

        if (!isComplete)
        {
            return new QualityComplianceScore
            {
                Score = 0,
                Rating = "Restricted",
                Summary = "Compliance score is restricted because the current user does not have access to all quality metrics.",
                IsComplete = false
            };
        }

        return new QualityComplianceScore
        {
            Score = score,
            Rating = GetRating(score),
            Summary = GetSummary(score),
            IsComplete = true
        };
    }

    private async Task<bool> HasPermissionAsync(string permissionKey)
    {
        return await _permissionAuthorization.HasPermissionAsync(
            _currentUser.UserId,
            _currentUser.Role,
            permissionKey);
    }

    private static string GetRating(int score)
    {
        if (score >= 95)
            return "Excellent";

        if (score >= 85)
            return "Good";

        if (score >= 70)
            return "Acceptable";

        if (score >= 50)
            return "High Risk";

        return "Critical";
    }

    private static string GetSummary(int score)
    {
        if (score >= 95)
            return "Quality system performing exceptionally well.";

        if (score >= 85)
            return "Minor corrective actions required.";

        if (score >= 70)
            return "Monitor quality performance closely.";

        if (score >= 50)
            return "Management intervention recommended.";

        return "Immediate corrective action required.";
    }
}

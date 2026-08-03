using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Web.Services.Quality;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityComplianceService
{
    private readonly WpsRepository _wpsService;
    private readonly PqrApplicationService _pqrService;
    private readonly WelderQualificationApplicationService _welderService;
    private readonly NdtApplicationService _ndtService;
    private readonly DocumentApplicationService _documentService;
    private readonly RepairApplicationService _repairService;

    public QualityComplianceService(
        WpsRepository wpsService,
        PqrApplicationService pqrService,
        WelderQualificationApplicationService welderService,
        NdtApplicationService ndtService,
        DocumentApplicationService documentService,
        RepairApplicationService repairService)
    {
        _wpsService = wpsService;
        _pqrService = pqrService;
        _welderService = welderService;
        _ndtService = ndtService;
        _documentService = documentService;
        _repairService = repairService;
    }

    public async Task<QualityComplianceScore> CalculateAsync()
    {
        var score = 0;

        // WPS
        if (_wpsService.GetActive().Count > 0)
            score += 20;

        // PQR
        if (_pqrService.GetActiveApprovedCount() > 0)
            score += 20;

        // Welder Qualifications
        if (_welderService.GetValidQualifiedWelderCount() > 0)
            score += 20;

        // Pending NDT
        var pending = _ndtService.GetPendingCount();

        score += pending switch
        {
            0 => 15,
            <= 5 => 12,
            <= 10 => 8,
            _ => 0
        };

        // Open Repairs
        var repairAnalytics = await _repairService.GetEnterpriseAnalytics();
        var repairs = repairAnalytics.OpenRepairs;

        score += repairs switch
        {
            0 => 10,
            <= 3 => 8,
            <= 10 => 5,
            _ => 0
        };

        // Missing Documents
        var missingDocs = _documentService.GetMissingDocumentCount();

        score += missingDocs switch
        {
            0 => 10,
            <= 5 => 8,
            <= 10 => 5,
            _ => 0
        };

        // Expiring Qualifications
        var expiring = _welderService.GetExpiringQualificationCount();

        score += expiring switch
        {
            0 => 5,
            <= 3 => 4,
            <= 10 => 2,
            _ => 0
        };

        return new QualityComplianceScore
        {
            Score = score,
            Rating = GetRating(score),
            Summary = GetSummary(score)
        };
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
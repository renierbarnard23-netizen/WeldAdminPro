using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityRecommendationService
{
    public List<string> Build(QualitySnapshot snapshot)
    {
        var recommendations = new List<string>();

        if (snapshot.ActiveWps == 0)
        {
            recommendations.Add(
                "Create and approve Welding Procedure Specifications (WPS) for active fabrication work.");
        }

        if (snapshot.ActivePqr == 0)
        {
            recommendations.Add(
                "Complete Procedure Qualification Records (PQRs) to support approved WPS documents.");
        }

        if (snapshot.MissingDocuments > 0)
        {
            recommendations.Add(
                $"Upload or approve the {snapshot.MissingDocuments} outstanding required project document(s).");
        }

        if (snapshot.PendingNdt > 0)
        {
            recommendations.Add(
                "Schedule pending NDT inspections to prevent production delays.");
        }

        if (snapshot.OpenRepairs > 0)
        {
            recommendations.Add(
                "Review and close outstanding weld repairs.");
        }

        if (snapshot.ExpiringQualifications > 0)
        {
            recommendations.Add(
                "Renew welder qualifications that will expire within the next 30 days.");
        }

        if (!recommendations.Any())
        {
            recommendations.Add(
                "Quality system performing within acceptable limits.");
        }

        return recommendations;
    }
}
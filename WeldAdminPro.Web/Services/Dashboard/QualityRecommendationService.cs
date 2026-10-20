using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityRecommendationService
{
    public List<string> Build(QualitySnapshot snapshot)
    {
        var recommendations = new List<string>();

        if (snapshot.ActiveWpsAvailable &&
            snapshot.ActiveWps == 0)
        {
            recommendations.Add("Establish or approve an applicable WPS.");
        }

        if (snapshot.ActivePqrAvailable &&
            snapshot.ActivePqr == 0)
        {
            recommendations.Add("Establish or approve supporting PQR documentation.");
        }

        if (snapshot.QualifiedWeldersAvailable &&
            snapshot.QualifiedWelders == 0)
        {
            recommendations.Add("Ensure qualified welders are available for production.");
        }

        if (snapshot.PendingNdtAvailable &&
            snapshot.PendingNdt > 0)
        {
            recommendations.Add($"Complete {snapshot.PendingNdt} pending NDT inspections.");
        }

        if (snapshot.OpenRepairsAvailable &&
            snapshot.OpenRepairs > 0)
        {
            recommendations.Add($"Review and close {snapshot.OpenRepairs} open repairs.");
        }

        if (snapshot.MissingDocumentsAvailable &&
            snapshot.MissingDocuments > 0)
        {
            recommendations.Add($"Complete {snapshot.MissingDocuments} missing quality documents.");
        }

        if (snapshot.ExpiringQualificationsAvailable &&
            snapshot.ExpiringQualifications > 0)
        {
            recommendations.Add($"Review {snapshot.ExpiringQualifications} welder qualifications expiring within 30 days.");
        }

        return recommendations;
    }
}

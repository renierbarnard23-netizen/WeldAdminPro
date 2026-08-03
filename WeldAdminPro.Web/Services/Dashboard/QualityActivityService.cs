using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityActivityService
{
    public List<string> Build(QualitySnapshot snapshot)
    {
        var activity = new List<string>();

        activity.Add($"Compliance score calculated: {snapshot.ComplianceScore}%");
        activity.Add($"Qualified welders: {snapshot.QualifiedWelders}");
        activity.Add($"Pending NDT inspections: {snapshot.PendingNdt}");
        activity.Add($"Open repairs: {snapshot.OpenRepairs}");
        activity.Add($"Missing documents: {snapshot.MissingDocuments}");

        return activity;
    }
}
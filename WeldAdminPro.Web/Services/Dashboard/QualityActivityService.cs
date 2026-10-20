using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityActivityService
{
    public List<string> Build(QualitySnapshot snapshot)
    {
        var activities = new List<string>();

        if (snapshot.IsComplete)
        {
            activities.Add($"Quality compliance score: {snapshot.ComplianceScore}%");
        }

        if (snapshot.QualifiedWeldersAvailable)
        {
            activities.Add($"Qualified welders: {snapshot.QualifiedWelders}");
        }

        if (snapshot.PendingNdtAvailable)
        {
            activities.Add($"Pending NDT inspections: {snapshot.PendingNdt}");
        }

        if (snapshot.OpenRepairsAvailable)
        {
            activities.Add($"Open repairs: {snapshot.OpenRepairs}");
        }

        if (snapshot.MissingDocumentsAvailable)
        {
            activities.Add($"Missing quality documents: {snapshot.MissingDocuments}");
        }

        return activities;
    }
}


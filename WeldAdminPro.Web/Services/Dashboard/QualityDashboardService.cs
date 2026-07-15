using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityDashboardService
{
    public QualitySnapshot GetDashboard()
    {
        var snapshot = new QualitySnapshot
        {
            ComplianceScore = 98,

            ActivePqr = 18,

            QualifiedWelders = 25,

            PendingNdt = 6,

            OpenRepairs = 2,

            MissingDocuments = 5,

            ExpiringQualifications = 3
        };

        try
        {
            snapshot.ActiveWps =
                _wpsRepository
                    .GetActive()
                    .Count;
        }
        catch
        {
            snapshot.ActiveWps = 0;
        }

        snapshot.Alerts.Add(
            "3 Welder qualifications expire within 30 days.");

        snapshot.Alerts.Add(
            "2 WPS documents require approval.");

        snapshot.Alerts.Add(
            "5 project documents are outstanding.");

        snapshot.RecentActivity.Add(
            "WPS-101 approved.");

        snapshot.RecentActivity.Add(
            "PQR-034 linked to WPS-101.");

        snapshot.RecentActivity.Add(
            "Repair completed on Project P24018.");

        snapshot.Recommendations.Add(
            "Schedule welder requalification.");

        snapshot.Recommendations.Add(
            "Approve outstanding WPS documents.");

        snapshot.Recommendations.Add(
            "Complete pending NDT inspections.");

        return snapshot;
    }

    private readonly WpsRepository _wpsRepository;

    public QualityDashboardService(
        WpsRepository wpsRepository)
    {
        _wpsRepository = wpsRepository;
    }

}

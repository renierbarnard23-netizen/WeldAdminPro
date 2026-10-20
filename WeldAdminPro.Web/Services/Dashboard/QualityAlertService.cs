using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityAlertService
{
    public QualityAlertSummary Build(QualitySnapshot snapshot)
    {
        var alerts = new List<QualityAlert>();

        if (snapshot.ComplianceScore < 70 &&
            !string.Equals(snapshot.ComplianceRating, "Restricted", StringComparison.OrdinalIgnoreCase))
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Critical,
                Message = "Quality compliance is below the acceptable threshold."
            });
        }

        if (snapshot.ActiveWpsAvailable &&
            snapshot.ActiveWps == 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Critical,
                Message = "No approved WPS available."
            });
        }

        if (snapshot.ActivePqrAvailable &&
            snapshot.ActivePqr == 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Critical,
                Message = "No approved PQR available."
            });
        }

        if (snapshot.MissingDocumentsAvailable &&
            snapshot.MissingDocuments > 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Warning,
                Message = $"{snapshot.MissingDocuments} required documents are missing."
            });
        }

        if (snapshot.PendingNdtAvailable &&
            snapshot.PendingNdt > 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Warning,
                Message = $"{snapshot.PendingNdt} pending NDT inspections."
            });
        }

        if (snapshot.OpenRepairsAvailable &&
            snapshot.OpenRepairs > 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Info,
                Message = $"{snapshot.OpenRepairs} open repairs."
            });
        }

        if (snapshot.ExpiringQualificationsAvailable &&
            snapshot.ExpiringQualifications > 0)
        {
            alerts.Add(new QualityAlert
            {
                Severity = QualityAlert.AlertSeverity.Warning,
                Message = $"{snapshot.ExpiringQualifications} welder qualifications expire within 30 days."
            });
        }

        return new QualityAlertSummary
        {
            Alerts = alerts
        };
    }
}
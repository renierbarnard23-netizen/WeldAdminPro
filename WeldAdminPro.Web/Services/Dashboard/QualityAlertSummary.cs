namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityAlertSummary
{
    public List<QualityAlert> Alerts { get; set; } = new();

    public int CriticalCount =>
        Alerts.Count(a => a.Severity == QualityAlert.AlertSeverity.Critical);

    public int WarningCount =>
        Alerts.Count(a => a.Severity == QualityAlert.AlertSeverity.Warning);

    public int InfoCount =>
        Alerts.Count(a => a.Severity == QualityAlert.AlertSeverity.Info);

    public bool HasAlerts => Alerts.Any();
}
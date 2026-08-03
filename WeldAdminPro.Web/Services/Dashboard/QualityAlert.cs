namespace WeldAdminPro.Web.Services.Dashboard;

public class QualityAlert
{
    public string Message { get; set; } = "";

    public AlertSeverity Severity { get; set; }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}
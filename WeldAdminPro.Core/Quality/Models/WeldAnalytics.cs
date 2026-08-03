namespace WeldAdminPro.Core.Quality.Models;

public class WeldAnalytics
{
    public int TotalWelds { get; set; }

    public int AcceptedWelds { get; set; }

    public int RejectedWelds { get; set; }

    public int PendingNdt { get; set; }

    public int RepairRequired { get; set; }

    public int CompletedWelds { get; set; }
}
namespace WeldAdminPro.Core.Quality.Models
{
    public class QaDashboardMetrics
    {
        public int TotalWelds { get; set; }

        public int AcceptedWelds { get; set; }

        public int WeldsUnderRepair { get; set; }

        public int PendingReinspection { get; set; }

        public int ClosedWelds { get; set; }

        public int OpenRepairs { get; set; }

        public int CompletedRepairs { get; set; }

        public int RejectedRepairs { get; set; }

        public double RepairRate { get; set; }
        public bool TurnoverReady { get; set; }
        public int BlockingIssues { get; set; }

    }
}

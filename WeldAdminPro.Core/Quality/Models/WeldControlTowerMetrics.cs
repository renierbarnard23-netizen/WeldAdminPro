namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldControlTowerMetrics
    {
        public int DraftCount { get; set; }

        public int NdtPendingCount { get; set; }

        public int RepairRequiredCount { get; set; }

        public int ReleasedCount { get; set; }

        public int ClosedCount { get; set; }

        public int BlockedCount { get; set; }

        public int OverdueNdtCount { get; set; }

        public int TotalWelds { get; set; }

        public double RepairRate { get; set; }

        public double FirstPassYield { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public double ReleaseRate { get; set; }
        public List<BlockedWeldItem> BlockedWelds
        {
            get;
            set;
        }
            = new();

        public List<ControlTowerWeldItem> FilteredWelds
        {
            get;
            set;
        }
            = new();

        public List<ControlTowerAlert> Alerts
        {
            get;
            set;
        }
= new();

        public List<DeadlineRiskItem> DeadlineRisks
        {
            get;
            set;
        }
= new();

    }
}
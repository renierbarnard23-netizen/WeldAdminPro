namespace WeldAdminPro.Core.Analytics.Production
{
    public class ProductionControlTowerModel
    {
        // =====================================
        // PRODUCTION STATUS
        // =====================================

        public int ReadyOrders { get; set; }

        public int RunningOrders { get; set; }

        public int BlockedOrders { get; set; }

        public int CompletedToday { get; set; }

        // =====================================
        // CAPACITY
        // =====================================

        public double CapacityLoad { get; set; }

        // =====================================
        // RISKS
        // =====================================

        public int DeadlineRisks { get; set; }

        public int MaterialShortages { get; set; }

        public int DelayedWorkOrders { get; set; }

        public int ActiveRepairs { get; set; }

        public int OverdueReservations { get; set; }

        public int BottleneckCount { get; set; }

        // =====================================
        // OVERALL STATUS
        // =====================================

        public string SystemStatus { get; set; }
            = "Healthy";

        public int TotalWorkOrders { get; set; }

        public int ReadyToStartToday { get; set; }

        public int OverdueWorkOrders { get; set; }

        public int HighPriorityOrders { get; set; }

        public int LateWorkOrders { get; set; }

        public int CompletedThisWeek { get; set; }

        public double ProductionEfficiency { get; set; }

        public List<ProductionAlert> Alerts
        {
            get;
            set;
        }
        = new();

        public int HealthScore
        {
            get;
            set;
        }

    }
}
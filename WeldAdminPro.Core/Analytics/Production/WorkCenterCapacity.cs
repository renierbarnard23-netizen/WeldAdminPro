namespace WeldAdminPro.Core.Analytics.Production
{
    public class WorkCenterCapacity
    {
        public string WorkCenter { get; set; } = "";

        public int ActiveWorkOrders { get; set; }

        public double ScheduledHours { get; set; }

        public double AvailableHours { get; set; }

        public double UtilizationPercent { get; set; }

        public bool IsOverloaded =>
            UtilizationPercent >= 100;

        public bool IsWarning =>
            UtilizationPercent >= 80 &&
            UtilizationPercent < 100;
    }
}
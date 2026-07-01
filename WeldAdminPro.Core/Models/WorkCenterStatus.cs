namespace WeldAdminPro.Core.Models
{
    public class WorkCenterStatus
    {
        public string Name { get; set; } = "";

        public bool IsRunning { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsIdle => !IsRunning && !IsBlocked;

        public string CurrentWorkOrder { get; set; } = "";

        public double UtilizationPercent { get; set; }

        public double HoursRemaining { get; set; }

        public int QueueLength { get; set; }

        public string StatusColor
        {
            get
            {
                if (IsBlocked)
                    return "#D32F2F"; // Red

                if (IsRunning)
                    return "#2E7D32"; // Green

                return "#F9A825"; // Amber
            }
        }
    }
}
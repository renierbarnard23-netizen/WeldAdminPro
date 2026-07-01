namespace WeldAdminPro.Core.Models
{
    public class WorkCenter
    {
        public string Name { get; set; } = "";

        public double HoursPerDay { get; set; } = 8;

        // 🔥 ADD THIS (REQUIRED)
        public double CurrentLoadHours { get; set; }

        // 🔥 ADD THIS (USEFUL FOR UI + RISK)
        public double CapacityUtilization =>
            HoursPerDay == 0 ? 0 : (CurrentLoadHours / HoursPerDay) * 100;

        public double LoadPercentage =>
        HoursPerDay == 0
            ? 0
            : Math.Min(
                100,
                (CurrentLoadHours / HoursPerDay) * 100);
    } 
}
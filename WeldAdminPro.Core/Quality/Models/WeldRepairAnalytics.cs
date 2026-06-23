namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldRepairAnalytics
    {
        public int TotalWelds { get; set; }

        public int TotalRepairs { get; set; }

        public double RepairRate { get; set; }

        public string MostFailedWelder { get; set; }
            = "";

        public string MostFailedWps { get; set; }
            = "";

        public int RepeatRepairWelds { get; set; }
    }
}
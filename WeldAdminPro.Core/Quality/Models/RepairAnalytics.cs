namespace WeldAdminPro.Core.Quality.Models
{
    public class RepairAnalytics
    {
        public int TotalRepairs { get; set; }

        public int OpenRepairs { get; set; }

        public int ClosedRepairs { get; set; }

        public double RepairRatePercent { get; set; }

        public double AverageClosureDays { get; set; }

        public string WorstWelder { get; set; }
            = "";

        public string WorstWps { get; set; }
            = "";
    }
}
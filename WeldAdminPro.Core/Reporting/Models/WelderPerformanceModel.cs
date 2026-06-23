namespace WeldAdminPro.Core.Quality.Models
{
    public class WelderPerformanceModel
    {
        public string WelderNumber
        { get; set; }
                = "";

        public int TotalWelds
        { get; set; }

        public int AcceptedWelds
        { get; set; }

        public int Repairs
        { get; set; }

        public int RepeatRepairs
        { get; set; }

        public double RepairRate
        { get; set; }

        public string WorstWps
        { get; set; }
                = "";

        public string RiskLevel
        { get; set; }
                = "LOW";
    }
}
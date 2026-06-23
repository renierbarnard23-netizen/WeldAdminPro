namespace WeldAdminPro.Core.Reporting.Models
{
    public class WelderPerformanceMetrics
    {
        public string WelderNumber { get; set; } = "";

        public int TotalWelds { get; set; }

        public int AcceptedWelds { get; set; }

        public int Repairs { get; set; }

        public int RejectedWelds { get; set; }

        public double RepairRate { get; set; }

        public double AcceptanceRate { get; set; }

        public int RepeatRepairs { get; set; }
    }
}
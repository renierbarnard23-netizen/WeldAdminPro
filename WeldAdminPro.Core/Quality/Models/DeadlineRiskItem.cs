namespace WeldAdminPro.Core.Quality.Models
{
    public class DeadlineRiskItem
    {
        public Guid WeldId { get; set; }

        public string WeldNumber { get; set; } = "";

        public string RiskLevel { get; set; } = "";

        public int DaysWaiting { get; set; }

        public string Reason { get; set; } = "";
    }
}
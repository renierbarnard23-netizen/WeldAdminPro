namespace WeldAdminPro.Core.Quality.Models
{
    public class RepairCostModel
    {
        public string WelderNumber
        { get; set; }
                = "";

        public int TotalRepairs
        { get; set; }

        public double EstimatedRepairHours
        { get; set; }

        public double EstimatedRepairCost
        { get; set; }

        public string RiskLevel
        { get; set; }
                = "LOW";
    }
}
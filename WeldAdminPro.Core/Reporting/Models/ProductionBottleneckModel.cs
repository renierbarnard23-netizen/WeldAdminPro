namespace WeldAdminPro.Core.Reporting.Models
{
    public class ProductionBottleneckModel
    {
        public string Category
        { get; set; }
                = "";

        public int Count
        { get; set; }

        public string RiskLevel
        { get; set; }
                = "LOW";

        public string Recommendation
        { get; set; }
                = "";
    }
}
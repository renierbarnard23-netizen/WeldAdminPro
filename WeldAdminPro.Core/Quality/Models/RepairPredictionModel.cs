namespace WeldAdminPro.Core.Quality.Models
{
    public class RepairPredictionModel
    {
        public string WelderNumber
        { get; set; }
                = "";

        public int TotalWelds
        { get; set; }

        public int Repairs
        { get; set; }

        public double RepairRate
        { get; set; }

        public string Prediction
        { get; set; }
                = "";

        public string RiskLevel
        { get; set; }
                = "LOW";

        public string Recommendation
        { get; set; }
                = "";
    }
}
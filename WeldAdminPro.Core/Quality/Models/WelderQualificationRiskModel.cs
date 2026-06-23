namespace WeldAdminPro.Core.Quality.Models
{
    public class WelderQualificationRiskModel
    {
        public string WelderNumber
        { get; set; }
                = "";

        public string Process
        { get; set; }
                = "";

        public string MaterialGroup
        { get; set; }
                = "";

        public int DaysRemaining
        { get; set; }

        public bool IsExpired
        { get; set; }

        public string RiskLevel
        { get; set; }
                = "LOW";
    }
}
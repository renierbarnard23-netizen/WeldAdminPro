namespace WeldAdminPro.Core.Quality.Models
{
    public class NcrLifecycleModel
    {
        public string NcrNumber
        { get; set; }
                = "";

        public string Status
        { get; set; }
                = "";

        public int AgeDays
        { get; set; }

        public bool IsOverdue
        { get; set; }

        public string RiskLevel
        { get; set; }
                = "LOW";
    }
}
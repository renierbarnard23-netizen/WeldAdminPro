namespace WeldAdminPro.Core.Quality.Models
{
    public class CapaAgingModel
    {
        public string Title
        { get; set; }
                = "";

        public string AssignedTo
        { get; set; }
                = "";

        public int AgeDays
        { get; set; }

        public int DaysRemaining
        { get; set; }

        public string Status
        { get; set; }
                = "";

        public string RiskLevel
        { get; set; }
                = "LOW";
    }
}
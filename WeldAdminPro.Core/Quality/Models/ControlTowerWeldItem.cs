namespace WeldAdminPro.Core.Quality.Models
{
    public class ControlTowerWeldItem
    {
        public Guid WeldId { get; set; }

        public string WeldNumber { get; set; } = "";

        public string WorkflowStatus { get; set; } = "";

        public string WpsNumber { get; set; } = "";

        public string WelderNumber { get; set; } = "";

        public string Summary { get; set; } = "";
    }
}
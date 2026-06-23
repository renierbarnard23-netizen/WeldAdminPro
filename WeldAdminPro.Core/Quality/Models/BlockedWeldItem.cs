namespace WeldAdminPro.Core.Quality.Models
{
    public class BlockedWeldItem
    {
        public Guid WeldId { get; set; }

        public string WeldNumber { get; set; } = "";

        public string WorkflowStatus { get; set; } = "";

        public string BlockingReason { get; set; } = "";
    }
}
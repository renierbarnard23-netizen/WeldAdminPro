namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldSummaryRow
    {
        public string WeldNumber { get; set; }
            = "";

        public string Wps { get; set; }
            = "";

        public string Welder { get; set; }
            = "";

        public string NdtStatus { get; set; }
            = "";

        public int Repairs { get; set; }

        public string WorkflowStatus { get; set; }
            = "";
    }
}
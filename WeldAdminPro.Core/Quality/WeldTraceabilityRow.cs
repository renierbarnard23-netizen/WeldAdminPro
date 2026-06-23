namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldTraceabilityRow
    {
        public string WeldNumber
        { get; set; } = "";

        public string Welder
        { get; set; } = "";

        public string WpsNumber
        { get; set; } = "";

        public string Material
        { get; set; } = "";

        public string NdtStatus
        { get; set; } = "";

        public string WeldStatus
        { get; set; } = "";

        public int RepairCount
        { get; set; }
    }
}

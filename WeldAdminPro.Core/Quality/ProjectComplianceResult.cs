namespace WeldAdminPro.Core.Quality
{
    public class ProjectComplianceResult
    {
        public bool IsCompliant { get; set; }

        public int WpsValid { get; set; }
        public int WpsInvalid { get; set; }

        public int WeldersExpired { get; set; }

        public int DocumentCompliancePercent { get; set; }

        public List<string> Issues { get; set; } = new();
    }
}
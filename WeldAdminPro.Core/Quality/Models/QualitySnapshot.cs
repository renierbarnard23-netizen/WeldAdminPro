namespace WeldAdminPro.Core.Quality.Models
{
    public class QualitySnapshot
    {
        // ==========================================
        // Executive KPIs
        // ==========================================

        public double ComplianceScore { get; set; }

        public int ActiveWps { get; set; }

        public int ActivePqr { get; set; }

        public int QualifiedWelders { get; set; }

        public int PendingNdt { get; set; }

        public int OpenRepairs { get; set; }

        public int MissingDocuments { get; set; }

        public int ExpiringQualifications { get; set; }

        // ==========================================
        // Dashboard
        // ==========================================

        public List<string> Alerts { get; set; } = new();

        public List<string> RecentActivity { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();

        public string ComplianceRating { get; set; } = "";

        public string ComplianceSummary { get; set; } = "";
    }
}
namespace WeldAdminPro.Core.Quality.Models
{
    public class QualitySnapshot
    {
        // ==========================================
        // Executive KPIs
        // ==========================================

        public double ComplianceScore { get; set; }

        public bool IsComplete { get; set; }

        public int ActiveWps { get; set; }

        public bool ActiveWpsAvailable { get; set; }

        public int ActivePqr { get; set; }

        public bool ActivePqrAvailable { get; set; }

        public int QualifiedWelders { get; set; }

        public bool QualifiedWeldersAvailable { get; set; }

        public int PendingNdt { get; set; }

        public bool PendingNdtAvailable { get; set; }

        public int OpenRepairs { get; set; }

        public bool OpenRepairsAvailable { get; set; }

        public List<RepairRecord> Repairs { get; set; } = new();

        public int MissingDocuments { get; set; }

        public bool MissingDocumentsAvailable { get; set; }

        public int ExpiringQualifications { get; set; }

        public bool ExpiringQualificationsAvailable { get; set; }

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


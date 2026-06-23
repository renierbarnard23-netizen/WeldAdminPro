using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Reporting.Models
{
    public class WeldDataBook
    {
        public string ProjectName { get; set; } = "";

        public string ClientName { get; set; } = "";

        public DateTime GeneratedDate { get; set; }

        public List<WeldDataBookEntry> Welds { get; set; }
            = new();

        public CompanyProfile Company { get; set; } = new();
        public WeldRepairAnalytics Analytics { get; set; } = new();

        public byte[]? RepairStatusChart { get; set; }

        public List<WelderPerformanceMetrics> WelderMetrics { get; set; } = new();
        public DataBookRevision RevisionInfo { get; set; } = new();
        public List<DataBookSection> Sections { get; set; } = new();
        public List<DataBookAttachment> Attachments { get; set; } = new();
        public List<WelderPerformanceModel> WelderPerformance { get; set; } = [];

        public List<RevisionHistoryEntry> RevisionHistory { get; set; } = [];
    }
}
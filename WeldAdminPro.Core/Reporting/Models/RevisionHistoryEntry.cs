using WeldAdminPro.Core.Reporting.Enums;

namespace WeldAdminPro.Core.Reporting.Models
{
    public class RevisionHistoryEntry
    {
        public string Revision { get; set; } = "";

        public DateTime RevisionDate { get; set; }

        public string Description { get; set; } = "";

        public string PreparedBy { get; set; } = "";

        public string ApprovedBy { get; set; } = "";

        public DocumentStatusType Status { get; set; }
    }
}

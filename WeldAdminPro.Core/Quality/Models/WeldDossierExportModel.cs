using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldDossierExportModel
    {
        public string WeldNumber { get; set; }
            = "";

        public List<string> WeldInfo { get; set; }
            = new();

        public List<string> NdtHistory { get; set; }
            = new();

        public List<string> NcrHistory { get; set; }
            = new();

        public List<WeldSummaryRow> WeldSummary
        {
            get;
            set;
        }
            = new();

        public List<NdtHistoryRow> NdtRows
        {
            get;
            set;
        }
            = new();

        public List<NcrHistoryRow> NcrRows
        {
            get;
            set;
        }
            =new();

        public List<RepairHistoryRow> RepairRows
        {
            get;
            set;
        }
            = new();

        public List<HoldPointSignatureRow> HoldPointRows
        {
            get;
            set;
        }
            = new();

        public List<AttachmentIndexRow> AttachmentRows
        {
            get;
            set;
        }
            = new();

        public List<QaSignoffRow> QaSignoffs
        {
            get;
            set;
        }
            = new();
    }
}
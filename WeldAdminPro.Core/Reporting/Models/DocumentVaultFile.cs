using PdfSharpCore.Pdf.IO;
using WeldAdminPro.Core.Reporting.Enums;

namespace WeldAdminPro.Core.Reporting.Models
{
    public class DocumentVaultFile
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = "";

        public string OriginalFileName { get; set; } = "";

        public string FilePath { get; set; } = "";

        public DocumentCategoryType Category { get; set; }

        public string Description { get; set; } = "";

        public string DocumentNumber { get; set; } = "";

        public string Title { get; set; } = "";

        public string Status { get; set; } = "Approved";

        public string Revision { get; set; } = "";

        public DateTime UploadedDate { get; set; }

        public string UploadedBy { get; set; } = "";

        public Guid? WeldId { get; set; }

        public Guid? ProjectId { get; set; }

        public bool IsApproved { get; set; }
    }
}

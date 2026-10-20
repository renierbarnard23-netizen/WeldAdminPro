using System;

namespace WeldAdminPro.Core.Models
{
    public class CompanyDocument
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public Guid CompanyId { get; set; }

        public string DocumentType { get; set; } = "";

        public string DocumentName { get; set; } = "";

        public string FileName { get; set; } = "";

        public string FilePath { get; set; } = "";

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        public bool RequiresRenewal { get; set; } = true;

        public int ReminderDays { get; set; } = 30;

        public bool IsNA { get; set; }

        public string NAReason { get; set; } = "";

        public DateTime UploadedDate { get; set; } =
            DateTime.UtcNow;

        public string UploadedBy { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
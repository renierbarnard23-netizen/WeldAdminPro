using System;

namespace WeldAdminPro.Core.Models
{
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public string DocType { get; set; } = string.Empty; // WPS/PQR/QCP
        public string Title { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Draft/Approved
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}

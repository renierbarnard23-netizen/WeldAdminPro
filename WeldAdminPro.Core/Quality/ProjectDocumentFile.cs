using System;

namespace WeldAdminPro.Core.Quality
{
    public class ProjectDocumentFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectDocumentId { get; set; }

        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
        public Guid DocumentId { get; set; }
        public bool IsApproved { get; set; }

    }
}
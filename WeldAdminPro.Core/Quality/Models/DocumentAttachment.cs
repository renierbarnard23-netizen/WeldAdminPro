using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class DocumentAttachment
    {
        public Guid Id { get; set; }

        public Guid RelatedEntityId { get; set; }

        public string EntityType { get; set; }
            = "";

        public string FileName { get; set; }
            = "";

        public string FilePath { get; set; }
            = "";

        public string UploadedBy { get; set; }
            = "";

        public DateTime UploadedDate { get; set; }

        public string Category { get; set; }
            = "";
    }
}
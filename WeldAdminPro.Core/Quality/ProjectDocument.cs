namespace WeldAdminPro.Core.Quality
{
    public class ProjectDocument
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsUploaded { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public DateTime? UploadedDate { get; set; }
    }
}
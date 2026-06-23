namespace WeldAdminPro.Core.Quality
{
    public class QualityDocumentRequirement
    {
        public int Id { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsUploaded { get; set; }

        public string? FilePath { get; set; }
    }
}
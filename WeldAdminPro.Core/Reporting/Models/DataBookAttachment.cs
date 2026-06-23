namespace WeldAdminPro.Core.Reporting.Models
{
    public class DataBookAttachment
    {
        public string FileName
        { get; set; } = "";

        public string FilePath
        { get; set; } = "";

        public string Category
        { get; set; } = "";

        public string Description
        { get; set; } = "";

        // NEW PROFESSIONAL FIELDS

        public string DocumentNumber
        { get; set; } = "";

        public string Title
        { get; set; } = "";

        public string Revision
        { get; set; } = "";

        public string Status
        { get; set; } = "";
    }
}

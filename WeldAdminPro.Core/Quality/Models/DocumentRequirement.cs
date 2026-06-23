using WeldAdminPro.Core.Reporting.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class DocumentRequirement
    {
        public DocumentCategoryType Category { get; set; }

        public string Description { get; set; }
        = string.Empty;

        public bool IsRequired { get; set; }

        public int MinimumRequired { get; set; }
    }
}

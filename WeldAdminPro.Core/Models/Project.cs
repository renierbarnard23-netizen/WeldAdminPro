namespace WeldAdminPro.Core.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        public string ProjectNumber { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}

using System;

namespace WeldAdminPro.Core.Models
{
    public class CompanyRequirement
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public Guid CompanyId { get; set; }

        public string RequirementCode { get; set; } = "";

        public string RequirementName { get; set; } = "";

        public string RequirementType { get; set; } = "";

        public string RequirementStatus { get; set; } = "";

        public string Notes { get; set; } = "";

        public DateTime? ReviewedDate { get; set; }

        public string ReviewedBy { get; set; } = "";

        public DateTime CreatedDate { get; set; } =
            DateTime.UtcNow;
    }
}
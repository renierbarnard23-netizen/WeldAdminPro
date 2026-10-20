using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public enum CustomerQualityRequirementStatus
    {
        Draft = 0,
        Active = 1,
        Superseded = 2,
        Closed = 3
    }

    public class CustomerQualityRequirement
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        public string RequirementNumber { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SourceDocument { get; set; } = string.Empty;

        public string SourceClause { get; set; } = string.Empty;

        public string Revision { get; set; } = string.Empty;

        public bool Mandatory { get; set; } = true;

        public string AcceptanceCriteria { get; set; } = string.Empty;

        public string VerificationMethod { get; set; } = string.Empty;

        public string ResponsibleParty { get; set; } = string.Empty;

        public string RequiredEvidence { get; set; } = string.Empty;

        public CustomerQualityRequirementStatus Status { get; set; }
            = CustomerQualityRequirementStatus.Draft;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ApprovedOn { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;

        public DateTime? EffectiveDate { get; set; }

        public DateTime? SupersededOn { get; set; }

        public Guid? PreviousRevisionId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}


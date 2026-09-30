using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class ProjectQualityControlPlanItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectQualityControlPlanId { get; set; }

        public int SequenceNumber { get; set; }

        public string Activity { get; set; } = string.Empty;

        public Guid? CustomerQualityRequirementId { get; set; }

        public string AcceptanceCriteria { get; set; } = string.Empty;

        public string VerificationMethod { get; set; } = string.Empty;

        public string ResponsibleParty { get; set; } = string.Empty;

        public string InspectionStage { get; set; } = string.Empty;

        public HoldPointCategory? HoldPointCategory { get; set; }

        public HoldPointType? HoldPointType { get; set; }

        public NdtMethodType? RequiredNdtMethod { get; set; }

        public bool Mandatory { get; set; }

        public string RequiredEvidence { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}

using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models;

public class QcpMasterActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AcceptanceCriteria { get; set; } = string.Empty;

    public string VerificationMethod { get; set; } = string.Empty;

    public string ResponsibleParty { get; set; } = string.Empty;

    public string InspectionStage { get; set; } = string.Empty;

    public HoldPointCategory? HoldPointCategory { get; set; }

    public HoldPointType? HoldPointType { get; set; }

    public NdtMethodType? RequiredNdtMethod { get; set; }

    public bool Mandatory { get; set; }

    public string RequiredEvidence { get; set; } = string.Empty;

    public string Applicability { get; set; } = string.Empty;

    public string ConditionType { get; set; } = string.Empty;

    public string SourceReference { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string ModifiedBy { get; set; } = string.Empty;

    public DateTime? ModifiedOn { get; set; }
}

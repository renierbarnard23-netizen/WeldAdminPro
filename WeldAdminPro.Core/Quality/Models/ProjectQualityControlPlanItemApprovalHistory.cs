using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models;

public class ProjectQualityControlPlanItemApprovalHistory
{
    public Guid Id { get; set; }

    public Guid ProjectQualityControlPlanItemId { get; set; }

    public QcpApprovalStatus Status { get; set; }

    public string Action { get; set; } =
        string.Empty;

    public string ApprovedBy { get; set; } =
        string.Empty;

    public DateTime ApprovedOn { get; set; }

    public string Remarks { get; set; } =
        string.Empty;
}

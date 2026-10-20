namespace WeldAdminPro.Core.Security.Models;

public class ProjectUserAssignment
{
    public Guid Id { get; set; } = Guid.Empty;

    public Guid ProjectId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

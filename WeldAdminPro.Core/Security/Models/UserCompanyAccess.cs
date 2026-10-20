namespace WeldAdminPro.Core.Security.Models;

public class UserCompanyAccess
{
    public Guid Id { get; set; } = Guid.Empty;

    public string UserId { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

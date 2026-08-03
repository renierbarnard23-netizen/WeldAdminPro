namespace WeldAdminPro.Core.Security.Models;

public class UserPermission
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    public bool IsGranted { get; set; }
}
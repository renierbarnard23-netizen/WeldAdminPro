namespace WeldAdminPro.Core.Security.Models;

public class RolePermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }
}
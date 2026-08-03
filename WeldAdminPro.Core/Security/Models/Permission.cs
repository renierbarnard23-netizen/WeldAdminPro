namespace WeldAdminPro.Core.Security.Models;

public class Permission
{
    public int Id { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public string PermissionGroup { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
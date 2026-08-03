using Microsoft.AspNetCore.Authorization;

namespace WeldAdminPro.Web.Security.Authorization;

public sealed class PermissionRequirement
    : IAuthorizationRequirement
{
    public PermissionRequirement(
        string permissionKey)
    {
        PermissionKey = permissionKey;
    }

    public string PermissionKey { get; }
}
namespace WeldAdminPro.Core.Security.Abstractions;

public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(
        string userId,
        string role,
        string permissionKey);
}

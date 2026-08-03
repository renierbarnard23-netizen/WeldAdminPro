namespace WeldAdminPro.Core.Security.Abstractions;

public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(
        string role,
        string permissionKey);
}
using System.Linq;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories.Security;

namespace WeldAdminPro.Data.Services.Security;

public class PermissionAuthorizationService
    : IPermissionAuthorizationService
{
    private readonly RoleRepository _roleRepository;
    private readonly PermissionRepository _permissionRepository;
    private readonly RolePermissionRepository _rolePermissionRepository;
    private readonly UserPermissionRepository _userPermissionRepository;

    public PermissionAuthorizationService(
        RoleRepository roleRepository,
        PermissionRepository permissionRepository,
        RolePermissionRepository rolePermissionRepository,
        UserPermissionRepository userPermissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _userPermissionRepository = userPermissionRepository;
    }

    public async Task<bool> HasPermissionAsync(
        string userId,
        string roleName,
        string permissionKey)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(roleName) ||
            string.IsNullOrWhiteSpace(permissionKey))
        {
            return false;
        }

        var permission =
            await _permissionRepository.GetByKeyAsync(
                permissionKey);

        if (permission == null)
            return false;

        var userPermissions =
            await _userPermissionRepository
                .GetByUserIdAsync(userId);

        var userOverride =
            userPermissions.FirstOrDefault(
                x => x.PermissionId == permission.Id);

        if (userOverride != null)
        {
            return userOverride.IsGranted;
        }

        var role =
            await _roleRepository.GetByNameAsync(
                roleName);

        if (role == null)
            return false;

        var permissionIds =
            await _rolePermissionRepository
                .GetPermissionIdsAsync(role.Id);

        return permissionIds.Contains(
            permission.Id);
    }
}

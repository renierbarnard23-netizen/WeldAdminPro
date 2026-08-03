using WeldAdminPro.Core.Security.Models;
using WeldAdminPro.Data.Repositories.Security;

namespace WeldAdminPro.Data.Services.Security;

public class PermissionAuthorizationService
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
    string roleName,
    string permissionKey)
    {
        var role =
            await _roleRepository.GetByNameAsync(roleName);

        if (role == null)
            return false;

        var permission =
            await _permissionRepository.GetByKeyAsync(permissionKey);

        if (permission == null)
            return false;

        var permissionIds =
            await _rolePermissionRepository
                .GetPermissionIdsAsync(role.Id);

        return permissionIds.Contains(permission.Id);
    }
}

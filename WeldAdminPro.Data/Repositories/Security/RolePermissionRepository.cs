using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Data.Repositories.Security;

public class RolePermissionRepository : RepositoryBase
{
    public RolePermissionRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId)
    {
        return await QueryAsync<RolePermission>(
            @"SELECT *
              FROM RolePermissions
              WHERE RoleId=@RoleId",
            new
            {
                RoleId = roleId
            });
    }

    public async Task<IEnumerable<int>> GetPermissionIdsAsync(int roleId)
    {
        return await QueryAsync<int>(
            @"SELECT PermissionId
              FROM RolePermissions
              WHERE RoleId=@RoleId",
            new
            {
                RoleId = roleId
            });
    }

    public async Task AddAsync(int roleId, int permissionId)
    {
        await ExecuteAsync(
            @"INSERT INTO RolePermissions
            (
                RoleId,
                PermissionId
            )
            VALUES
            (
                @RoleId,
                @PermissionId
            )",
            new
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
    }

    public async Task DeleteByRoleAsync(int roleId)
    {
        await ExecuteAsync(
            @"DELETE
              FROM RolePermissions
              WHERE RoleId=@RoleId",
            new
            {
                RoleId = roleId
            });
    }
}
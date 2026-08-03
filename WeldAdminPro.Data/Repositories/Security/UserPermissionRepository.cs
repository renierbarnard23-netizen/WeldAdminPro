using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Data.Repositories.Security;

public class UserPermissionRepository : RepositoryBase
{
    public UserPermissionRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<UserPermission>> GetByUserIdAsync(string userId)
    {
        return await QueryAsync<UserPermission>(
            @"SELECT *
              FROM UserPermissions
              WHERE UserId=@UserId",
            new
            {
                UserId = userId
            });
    }

    public async Task SaveAsync(UserPermission permission)
    {
        await ExecuteAsync(
            @"
INSERT OR REPLACE INTO UserPermissions
(
    UserId,
    PermissionId,
    IsGranted
)
VALUES
(
    @UserId,
    @PermissionId,
    @IsGranted
)",
            permission);
    }

    public async Task DeleteByUserAsync(string userId)
    {
        await ExecuteAsync(
            @"DELETE
              FROM UserPermissions
              WHERE UserId=@UserId",
            new
            {
                UserId = userId
            });
    }
}
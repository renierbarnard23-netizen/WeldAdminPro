using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Data.Repositories.Security;

public class PermissionRepository : RepositoryBase
{
    public PermissionRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await QueryAsync<Permission>(
            @"SELECT *
              FROM Permissions
              ORDER BY PermissionGroup, Name");
    }

    public async Task<Permission?> GetByKeyAsync(string key)
    {
        return await QuerySingleAsync<Permission>(
            @"SELECT *
              FROM Permissions
              WHERE PermissionKey=@Key",
            new
            {
                Key = key
            });
    }
}
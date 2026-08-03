using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Data.Repositories.Security;

public class RoleRepository : RepositoryBase
{
    public RoleRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await QueryAsync<Role>(
            @"SELECT *
              FROM Roles
              ORDER BY Name");
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await QuerySingleAsync<Role>(
            @"SELECT *
              FROM Roles
              WHERE Id=@Id",
            new
            {
                Id = id
            });
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await QuerySingleAsync<Role>(
            @"SELECT *
              FROM Roles
              WHERE Name=@Name",
            new
            {
                Name = name
            });
    }
}
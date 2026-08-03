using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Security.Catalog;

namespace WeldAdminPro.Data.Services.Security;

public static class SecuritySeeder
{
    public static void Seed(SqliteConnection connection)
    {
        SeedRoles(connection);
        SeedPermissions(connection);
        SeedRolePermissions(connection);
    }

    private static void SeedRoles(
        SqliteConnection connection)
    {
        foreach (var role in RoleCatalog.All)
        {
            var existingId =
                connection.ExecuteScalar<int?>(
                    @"
SELECT Id
FROM Roles
WHERE Name = @Name",
                    new
                    {
                        role.Name
                    });

            if (existingId.HasValue)
            {
                connection.Execute(
                    @"
UPDATE Roles
SET
    Description = @Description,
    IsSystemRole = @IsSystemRole
WHERE Id = @Id",
                    new
                    {
                        Id = existingId.Value,
                        role.Description,
                        IsSystemRole = role.IsSystemRole ? 1 : 0
                    });

                continue;
            }

            connection.Execute(
                @"
INSERT INTO Roles
(
    Name,
    Description,
    IsSystemRole
)
VALUES
(
    @Name,
    @Description,
    @IsSystemRole
)",
                new
                {
                    role.Name,
                    role.Description,
                    IsSystemRole = role.IsSystemRole ? 1 : 0
                });
        }
    }

    private static void SeedPermissions(
        SqliteConnection connection)
    {
        foreach (var permission in PermissionCatalog.All)
        {
            var existingId =
                connection.ExecuteScalar<int?>(
                    @"
SELECT Id
FROM Permissions
WHERE PermissionKey = @PermissionKey",
                    new
                    {
                        PermissionKey = permission.Key
                    });

            if (existingId.HasValue)
            {
                connection.Execute(
                    @"
UPDATE Permissions
SET
    PermissionGroup = @PermissionGroup,
    Name = @Name,
    Description = @Description
WHERE Id = @Id",
                    new
                    {
                        Id = existingId.Value,
                        PermissionGroup = permission.Group,
                        permission.Name,
                        permission.Description
                    });

                continue;
            }

            connection.Execute(
                @"
INSERT INTO Permissions
(
    PermissionKey,
    PermissionGroup,
    Name,
    Description
)
VALUES
(
    @PermissionKey,
    @PermissionGroup,
    @Name,
    @Description
)",
                new
                {
                    PermissionKey = permission.Key,
                    PermissionGroup = permission.Group,
                    permission.Name,
                    permission.Description
                });
        }
    }

    private static void SeedRolePermissions(
        SqliteConnection connection)
    {
        foreach (var roleEntry in RolePermissionCatalog.All)
        {
            var roleName = roleEntry.Key;
            var permissionKeys = roleEntry.Value;

            var roleId =
                connection.ExecuteScalar<int?>(
                    @"
SELECT Id
FROM Roles
WHERE Name = @RoleName",
                    new
                    {
                        RoleName = roleName
                    });

            if (!roleId.HasValue)
            {
                Console.WriteLine(
                    $"Security seed warning: role '{roleName}' was not found.");

                continue;
            }

            // System role permissions are defined by
            // RolePermissionCatalog, so rebuild them
            // from the authoritative permission matrix.
            connection.Execute(
                @"
DELETE FROM RolePermissions
WHERE RoleId = @RoleId",
                new
                {
                    RoleId = roleId.Value
                });

            foreach (var permissionKey in permissionKeys.Distinct())
            {
                var permissionId =
                    connection.ExecuteScalar<int?>(
                        @"
SELECT Id
FROM Permissions
WHERE PermissionKey = @PermissionKey",
                        new
                        {
                            PermissionKey = permissionKey
                        });

                if (!permissionId.HasValue)
                {
                    Console.WriteLine(
                        $"Security seed warning: permission '{permissionKey}' " +
                        $"for role '{roleName}' was not found.");

                    continue;
                }

                connection.Execute(
                    @"
INSERT INTO RolePermissions
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
                        RoleId = roleId.Value,
                        PermissionId = permissionId.Value
                    });
            }
        }
    }
}
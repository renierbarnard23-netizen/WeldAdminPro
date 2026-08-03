using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Security.Catalog;
using System.Linq;

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
            var exists = connection.ExecuteScalar<int>(
                @"SELECT COUNT(*)
              FROM Roles
              WHERE Name=@Name",
                new
                {
                    role.Name
                });

            if (exists > 0)
                continue;

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
            var exists = connection.ExecuteScalar<int>(
                @"SELECT COUNT(*)
              FROM Permissions
              WHERE PermissionKey=@PermissionKey",
                new
                {
                    PermissionKey = permission.Key
                });

            if (exists > 0)
                continue;

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
        // Get Administrator Role Id
        var administratorRoleId =
            connection.ExecuteScalar<int?>(
                @"SELECT Id
              FROM Roles
              WHERE Name = 'Administrator'");

        if (administratorRoleId == null)
            return;

        // Get every permission
        var permissions =
            connection.Query<int>(
                @"SELECT Id
              FROM Permissions")
            .ToList();

        foreach (var permissionId in permissions)
        {
            var exists =
                connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*)
                  FROM RolePermissions
                  WHERE RoleId=@RoleId
                  AND PermissionId=@PermissionId",
                    new
                    {
                        RoleId = administratorRoleId,
                        PermissionId = permissionId
                    });

            if (exists > 0)
                continue;

            connection.Execute(
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
                    RoleId = administratorRoleId,
                    PermissionId = permissionId
                });
        }
    }
}
using Dapper;
using Microsoft.Data.Sqlite;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class SystemUserRepository
    {
        private readonly string _connectionString;

        public SystemUserRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(SystemUser user)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"
INSERT INTO SystemUsers
(
    Id,
    Username,
    PasswordHash,
    FullName,
    Email,
    Role,
    RoleId,
    IsActive,
    CreatedDate,
    LastLoginDate
)
VALUES
(
    @Id,
    @Username,
    @PasswordHash,
    @FullName,
    @Email,
    0,
    @RoleId,
    @IsActive,
    @CreatedDate,
    @LastLoginDate
)",
                new
                {
                    Id =
                        user.Id.ToString(),

                    user.Username,

                    user.PasswordHash,

                    user.FullName,

                    user.Email,

                    user.RoleId,

                    IsActive =
                        user.IsActive ? 1 : 0,

                    CreatedDate =
                        user.CreatedDate.ToString("O"),

                    LastLoginDate =
                        user.LastLoginDate?.ToString("O")
                });
        }

        public SystemUser? GetByUsername(
            string username)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var row =
                connection.QueryFirstOrDefault(
                    @"
SELECT
    u.Id,
    u.Username,
    u.PasswordHash,
    u.FullName,
    u.Email,
    u.RoleId,
    u.IsActive,
    u.CreatedDate,
    u.LastLoginDate,
    r.Name AS RoleName
FROM SystemUsers u
LEFT JOIN Roles r
    ON r.Id = u.RoleId
WHERE u.Username = @Username",
                    new
                    {
                        Username = username
                    });

            if (row == null)
            {
                return null;
            }

            return MapUser(row);
        }

        public List<SystemUser> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    @"
SELECT
    u.Id,
    u.Username,
    u.PasswordHash,
    u.FullName,
    u.Email,
    u.RoleId,
    u.IsActive,
    u.CreatedDate,
    u.LastLoginDate,
    r.Name AS RoleName
FROM SystemUsers u
LEFT JOIN Roles r
    ON r.Id = u.RoleId
ORDER BY u.Username");

            return rows
                .Select(MapUser)
                .ToList();
        }

        public void Update(SystemUser user)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"
UPDATE SystemUsers
SET
    Username = @Username,
    PasswordHash = @PasswordHash,
    FullName = @FullName,
    Email = @Email,
    RoleId = @RoleId,
    IsActive = @IsActive,
    LastLoginDate = @LastLoginDate
WHERE Id = @Id",
                new
                {
                    Id =
                        user.Id.ToString(),

                    user.Username,

                    user.PasswordHash,

                    user.FullName,

                    user.Email,

                    user.RoleId,

                    IsActive =
                        user.IsActive ? 1 : 0,

                    LastLoginDate =
                        user.LastLoginDate?.ToString("O")
                });
        }

        private static SystemUser MapUser(
            dynamic row)
        {
            return new SystemUser
            {
                Id =
                    Guid.Parse(
                        (string)row.Id),

                Username =
                    row.Username,

                PasswordHash =
                    row.PasswordHash,

                FullName =
                    row.FullName,

                Email =
                    row.Email ?? "",

                RoleId =
                    row.RoleId == null
                        ? 0
                        : (int)(long)row.RoleId,

                RoleName =
                    row.RoleName ?? "",

                IsActive =
                    row.IsActive == 1,

                CreatedDate =
                    DateTime.Parse(
                        (string)row.CreatedDate),

                LastLoginDate =
                    row.LastLoginDate != null
                        ? DateTime.Parse(
                            (string)row.LastLoginDate)
                        : null
            };
        }
    }
}
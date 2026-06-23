using Dapper;
using Microsoft.Data.Sqlite;
using PdfSharpCore.Pdf.IO;
using System.Linq;
using WeldAdminPro.Core.Enums;
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
                new SqliteConnection(_connectionString);

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
    @Role,
    @IsActive,
    @CreatedDate,
    @LastLoginDate
)",
                new
                {
                    Id = user.Id.ToString(),

                    user.Username,

                    user.PasswordHash,

                    user.FullName,

                    user.Email,

                    Role = (int)user.Role,

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
SELECT *
FROM SystemUsers
WHERE Username = @Username",
                    new
                    {
                        Username = username
                    });

            if (row == null)
            {
                return null;
            }

            return new SystemUser
            {
                Id =
                    Guid.Parse(row.Id),

                Username =
                    row.Username,

                PasswordHash =
                    row.PasswordHash,

                FullName =
                    row.FullName,

                Email =
                    row.Email ?? "",

                Role =
                    (SystemRole)row.Role,

                IsActive =
                    row.IsActive == 1,

                CreatedDate =
                    DateTime.Parse(row.CreatedDate),

                LastLoginDate =
                    row.LastLoginDate != null
                        ? DateTime.Parse(row.LastLoginDate)
                        : null
            };
        }

public List<SystemUser> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    "SELECT * FROM SystemUsers");

            return rows.Select(row =>
                new SystemUser
                {
                    Id =
                        Guid.Parse(row.Id),

                    Username =
                        row.Username,

                    PasswordHash =
                        row.PasswordHash,

                    FullName =
                        row.FullName,

                    Email =
                        row.Email ?? "",

                    Role =
                        (SystemRole)row.Role,

                    IsActive =
                        row.IsActive == 1,

                    CreatedDate =
                        DateTime.Parse(row.CreatedDate),

                    LastLoginDate =
                        row.LastLoginDate != null
                            ? DateTime.Parse(row.LastLoginDate)
                            : null
                })
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
    Role = @Role,
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

                    Role =
                        (int)user.Role,

                    IsActive =
                        user.IsActive ? 1 : 0,

                    LastLoginDate =
                        user.LastLoginDate?.ToString("O")
                });
        }


    }
}

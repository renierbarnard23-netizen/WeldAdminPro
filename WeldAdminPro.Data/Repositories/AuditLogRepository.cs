using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class AuditLogRepository
    {
        private readonly string _connectionString;

        public AuditLogRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
    AuditLog log)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            connection.Execute(
    @"
INSERT INTO AuditLogs
(
    Id,
    Username,
    Action,
    Module,
    Details,
    Timestamp
)
VALUES
(
    @Id,
    @Username,
    @Action,
    @Module,
    @Details,
    @Timestamp
)",
    new
    {
        Id =
            log.Id.ToString(),

        Username =
            log.Username,

        Action =
            log.Action,

        Module =
            log.Module,

        Details =
            log.Details,

        Timestamp =
            log.Timestamp
                .ToString("O")
    });
        }

        public List<AuditLog> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    "SELECT * FROM AuditLogs ORDER BY Timestamp DESC");

            return rows.Select(row =>
                new AuditLog
                {
                    Id =
                        Guid.Parse(row.Id),

                    Username =
                        row.Username,

                    Action =
                        row.Action,

                    Module =
                        row.Module,

                    Details =
                        row.Details,

                    Timestamp =
                        DateTime.Parse(
                            row.Timestamp.ToString())
                })
                .ToList();
        }
    }
}
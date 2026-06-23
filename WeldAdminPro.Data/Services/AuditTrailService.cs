using Dapper;
using Microsoft.Data.Sqlite;
using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
    public class AuditTrailService
    {
        private readonly string _connectionString;

        public AuditTrailService(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Log(
            AuditTrailEntry entry)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"
INSERT INTO AuditTrailEntries
(
    Id,
    Timestamp,
    UserName,
    Module,
    Action,
    EntityType,
    EntityId,
    Details
)
VALUES
(
    @Id,
    @Timestamp,
    @UserName,
    @Module,
    @Action,
    @EntityType,
    @EntityId,
    @Details
)",
                new
                {
                    Id =
                        entry.Id.ToString(),

                    Timestamp =
                        entry.Timestamp.ToString("O"),

                    entry.UserName,

                    entry.Module,

                    entry.Action,

                    entry.EntityType,

                    entry.EntityId,

                    entry.Details
                });
        }
    }
}

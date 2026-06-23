using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;

namespace WeldAdminPro.Data.Services
{
    public class DatabaseMigrationService
    {
        private readonly string _connectionString;

        public DatabaseMigrationService(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void ApplyMigrations()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var version =
                connection.QueryFirstOrDefault<int?>(
                    @"
SELECT MAX(SchemaVersion)
FROM DatabaseVersions");

            var currentVersion =
                version ?? 0;

            if (currentVersion < 1)
            {
                ApplyVersion1(connection);
            }

            if (currentVersion < 2)
            {
                ApplyVersion2(connection);
            }

            if (currentVersion < 3)
            {
                ApplyVersion3(connection);
            }

            if (currentVersion < 4)
            {
                ApplyVersion4(connection);
            }
        }

        // =====================================
        // VERSION 1
        // =====================================

        private void ApplyVersion1(
            SqliteConnection connection)
        {
            connection.Execute(
                @"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
VALUES
(
    1,
    '1.0.0',
    @AppliedDate,
    'Initial schema'
)",
                new
                {
                    AppliedDate =
                        DateTime.Now.ToString("O")
                });
        }

        // =====================================
        // VERSION 2
        // =====================================

        private void ApplyVersion2(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "CapaRecords",
                "Title",
                "TEXT");

            TryAddColumn(
                connection,
                "CapaRecords",
                "CreatedBy",
                "TEXT");

            TryAddColumn(
                connection,
                "CapaRecords",
                "VerifiedBy",
                "TEXT");

            TryAddColumn(
                connection,
                "CapaRecords",
                "VerifiedDate",
                "TEXT");

            TryAddColumn(
                connection,
                "CapaRecords",
                "IsEffective",
                "INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "CapaRecords",
                "Priority",
                "INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "CapaRecords",
                "Status",
                "INTEGER DEFAULT 0");

            connection.Execute(
                @"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
VALUES
(
    2,
    '1.1.0',
    @AppliedDate,
    'MRB + CAPA system added'
)",
                new
                {
                    AppliedDate =
                        DateTime.Now.ToString("O")
                });
        }

        private bool TableExists(
    SqliteConnection connection,
    string table)
        {
            var count =
                connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*)
              FROM sqlite_master
              WHERE type='table'
              AND name=@table",
                    new { table });

            return count > 0;
        }


        private void RecordVersion(
    SqliteConnection connection,
    int version,
    string build,
    string notes)
        {
            connection.Execute(
                @"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
VALUES
(
    @Version,
    @Build,
    @Date,
    @Notes
)",
                new
                {
                    Version = version,
                    Build = build,
                    Date = DateTime.UtcNow.ToString("O"),
                    Notes = notes
                });
        }

        // =====================================
        // VERSION 3
        // =====================================

        private void ApplyVersion3(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "NcrRecords",
                "NcrNumber",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "RootCause",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "CorrectiveAction",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "PreventiveAction",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "AssignedTo",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "DueDate",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "DispositionType",
                "INTEGER");

            TryAddColumn(
                connection,
                "NcrRecords",
                "DispositionApprovedBy",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "DispositionApprovedDate",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "VerificationBy",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "VerificationDate",
                "TEXT");

            TryAddColumn(
                connection,
                "NcrRecords",
                "RequiresCustomerApproval",
                "INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "NcrRecords",
                "CustomerApproved",
                "INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "NcrRecords",
                "CustomerApprovalReference",
                "TEXT");

            connection.Execute(
                @"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
VALUES
(
    3,
    '1.2.0',
    @AppliedDate,
    'Expanded NCR system'
)",
                new
                {
                    AppliedDate =
                        DateTime.Now.ToString("O")
                });
        }

        // =====================================
        // VERSION 4
        // =====================================

        private void ApplyVersion4(
            SqliteConnection connection)
        {
            // =====================================
            // REPAIR RECORD MIGRATIONS
            // =====================================

            TryAddColumn(
                connection,
                "RepairRecords",
                "CompletedDate",
                "TEXT");

            TryAddColumn(
                connection,
                "RepairRecords",
                "ApprovedBy",
                "TEXT");

            TryAddColumn(
                connection,
                "RepairRecords",
                "ApprovedDate",
                "TEXT");

            TryAddColumn(
                connection,
                "RepairRecords",
                "RepairWpsNumber",
                "TEXT");

            TryAddColumn(
                connection,
                "RepairRecords",
                "RepairedByWelder",
                "TEXT");

            // =====================================
            // VERSION RECORD
            // =====================================

            connection.Execute(
                @"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
VALUES
(
    4,
    '1.3.0',
    @AppliedDate,
    'Expanded Repair Management system'
)",
                new
                {
                    AppliedDate =
                        DateTime.Now.ToString("O")
                });
        }

        // =====================================
        // SAFE COLUMN ADDER
        // =====================================

        private void TryAddColumn(
            SqliteConnection connection,
            string table,
            string column,
            string definition)
        {
            var exists =
                connection.Query(
                    $"PRAGMA table_info({table})")
                .Any(x =>
                    x.name.ToString() == column);

            if (!exists)
            {
                connection.Execute(
                    $"ALTER TABLE {table} ADD COLUMN {column} {definition}");
            }
        }
    }
}
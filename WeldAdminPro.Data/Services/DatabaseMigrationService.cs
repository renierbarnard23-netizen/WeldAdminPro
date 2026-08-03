using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using WeldAdminPro.Core.Security.Catalog;
using WeldAdminPro.Data.Services.Security;

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

            int currentVersion = 0;

            if (TableExists(connection, "DatabaseVersions"))
            {
                currentVersion =
                    connection.QueryFirstOrDefault<int?>(
                        @"SELECT MAX(SchemaVersion)
              FROM DatabaseVersions")
                    ?? 0;
            }

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

            if (currentVersion < 5)
            {
                ApplyVersion5(connection);
            }
        }

        // =====================================
        // VERSION 1
        // =====================================

        private void ApplyVersion1(
            SqliteConnection connection)
        {
            RecordVersion(
                connection,
                1,
                "1.0.0",
                "Initial schema");
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

            RecordVersion(
                connection,
                2,
                "1.1.0",
                "MRB + CAPA system added");
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

            RecordVersion(
                connection,
                3,
                "1.2.0",
                "Expanded NCR system");
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

            RecordVersion(
                connection,
                4,
                "1.3.0",
                "Expanded Repair Management system");


        }

        // =====================================
        // VERSION 5
        // Enterprise Security Framework
        // =====================================

        private void ApplyVersion5(SqliteConnection connection)
        {
            CreateRolesTable(connection);
            CreatePermissionsTable(connection);
            CreateRolePermissionsTable(connection);
            CreateUserPermissionsTable(connection);

            SecuritySeeder.Seed(connection);
            
            RecordVersion(
                connection,
                5,
                "1.4.0",
                "Enterprise Security Framework");
        }

        private void CreateRolesTable(
    SqliteConnection connection)
        {
            if (TableExists(connection, "Roles"))
                return;

            connection.Execute(
                @"
CREATE TABLE Roles
(
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL UNIQUE,
    Description     TEXT,
    IsSystemRole    INTEGER NOT NULL DEFAULT 0
);");
        }

        private void CreatePermissionsTable(
    SqliteConnection connection)
        {
            if (TableExists(connection, "Permissions"))
                return;

            connection.Execute(
                @"
CREATE TABLE Permissions
(
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    PermissionKey   TEXT NOT NULL UNIQUE,
    PermissionGroup TEXT NOT NULL,
    Name            TEXT NOT NULL,
    Description     TEXT
);");
        }

        private void CreateRolePermissionsTable(
    SqliteConnection connection)
        {
            if (TableExists(connection, "RolePermissions"))
                return;

            connection.Execute(
                @"
CREATE TABLE RolePermissions
(
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,

    RoleId          INTEGER NOT NULL,

    PermissionId    INTEGER NOT NULL,

    FOREIGN KEY(RoleId)
        REFERENCES Roles(Id)
        ON DELETE CASCADE,

    FOREIGN KEY(PermissionId)
        REFERENCES Permissions(Id)
        ON DELETE CASCADE,

    UNIQUE(RoleId, PermissionId)
);");
        }

        private void CreateUserPermissionsTable(
    SqliteConnection connection)
        {
            if (TableExists(connection, "UserPermissions"))
                return;

            connection.Execute(
                @"
CREATE TABLE UserPermissions
(
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,

    UserId              TEXT NOT NULL,

    PermissionId        INTEGER NOT NULL,

    IsGranted           INTEGER NOT NULL,

    FOREIGN KEY(UserId)
        REFERENCES SystemUsers(Id)
        ON DELETE CASCADE,

    FOREIGN KEY(PermissionId)
        REFERENCES Permissions(Id)
        ON DELETE CASCADE,

    UNIQUE(UserId, PermissionId)
);");
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
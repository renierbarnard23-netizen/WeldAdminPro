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

            if (currentVersion < 6)
            {
                ApplyVersion6(connection);
            }

            if (currentVersion < 7)
            {
                ApplyVersion7(connection);
            }

            // =====================================
            // SECURITY CATALOG SYNCHRONIZATION
            // =====================================
            //
            // Roles, permissions and the system-role permission
            // matrix are application reference data rather than
            // database schema. Synchronize them on startup so
            // changes to the security catalogue are applied to
            // existing databases as well.
            //
            if (TableExists(connection, "Roles") &&
                TableExists(connection, "Permissions") &&
                TableExists(connection, "RolePermissions") &&
                TableExists(connection, "UserPermissions"))
            {
                SecuritySeeder.Seed(connection);
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
        // VERSION 6
        // Database-backed System User Roles
        // =====================================

        private void ApplyVersion6(
            SqliteConnection connection)
        {
            // Ensure the enterprise security catalogue is
            // available before mapping legacy user roles.
            SecuritySeeder.Seed(connection);

            // Keep the legacy Role column during the
            // transition. RoleId becomes the new role link.
            TryAddColumn(
                connection,
                "SystemUsers",
                "RoleId",
                "INTEGER");

            // =====================================
            // LEGACY SYSTEMROLE -> ROLES TABLE
            // =====================================
            //
            // Legacy values:
            //
            // 0 = Viewer
            // 1 = Welder
            // 2 = QC
            // 3 = QA
            // 4 = Supervisor
            // 5 = WeldingCoordinator
            // 6 = QualityManager
            // 7 = OperationsManager
            // 8 = StoreController
            // 9 = Admin
            //
            // Resolve the new RoleId by role NAME rather
            // than relying on database-generated IDs.
            //

            connection.Execute(
                @"
UPDATE SystemUsers
SET RoleId =
    CASE Role

        WHEN 0 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Viewer'
            )

        WHEN 1 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Welder'
            )

        WHEN 2 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'QC Inspector'
            )

        WHEN 3 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'QA Inspector'
            )

        WHEN 4 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Production Supervisor'
            )

        WHEN 5 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Welding Coordinator'
            )

        WHEN 6 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Quality Manager'
            )

        WHEN 7 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Operations Manager'
            )

        WHEN 8 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Store Controller'
            )

        WHEN 9 THEN
            (
                SELECT Id
                FROM Roles
                WHERE Name = 'Administrator'
            )

        ELSE NULL
    END
WHERE RoleId IS NULL;
");

            // =====================================
            // MIGRATION VALIDATION
            // =====================================

            var unmappedUsers =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM SystemUsers
WHERE RoleId IS NULL;
");

            if (unmappedUsers > 0)
            {
                throw new InvalidOperationException(
                    $"Version 6 migration failed: " +
                    $"{unmappedUsers} SystemUsers could not be mapped to Roles.");
            }

            RecordVersion(
                connection,
                6,
                "1.5.0",
                "Database-backed System User roles");
        }


        // =====================================
        // VERSION 7
        // Remove obsolete security roles
        // =====================================

        private void ApplyVersion7(
            SqliteConnection connection)
        {
            // Ensure the authoritative security catalogue
            // is synchronized before cleanup.
            SecuritySeeder.Seed(connection);

            // =====================================
            // SAFETY VALIDATION
            // =====================================
            //
            // Obsolete roles must never be deleted while
            // they are still assigned to system users.
            //

            var assignedObsoleteUsers =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM SystemUsers u
INNER JOIN Roles r
    ON r.Id = u.RoleId
WHERE r.Name IN
(
    'Production Manager',
    'Project Manager',
    'Engineer',
    'Supervisor'
);
");

            if (assignedObsoleteUsers > 0)
            {
                throw new InvalidOperationException(
                    "Version 7 migration failed: " +
                    $"{assignedObsoleteUsers} SystemUsers are still assigned " +
                    "to obsolete security roles.");
            }

            // =====================================
            // REMOVE OBSOLETE ROLE PERMISSIONS
            // =====================================

            connection.Execute(
                @"
DELETE FROM RolePermissions
WHERE RoleId IN
(
    SELECT Id
    FROM Roles
    WHERE Name IN
    (
        'Production Manager',
        'Project Manager',
        'Engineer',
        'Supervisor'
    )
);
");

            // =====================================
            // REMOVE OBSOLETE ROLES
            // =====================================

            connection.Execute(
                @"
DELETE FROM Roles
WHERE Name IN
(
    'Production Manager',
    'Project Manager',
    'Engineer',
    'Supervisor'
);
");

            // =====================================
            // VALIDATE CLEANUP
            // =====================================

            var remainingObsoleteRoles =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Roles
WHERE Name IN
(
    'Production Manager',
    'Project Manager',
    'Engineer',
    'Supervisor'
);
");

            if (remainingObsoleteRoles > 0)
            {
                throw new InvalidOperationException(
                    "Version 7 migration failed: " +
                    $"{remainingObsoleteRoles} obsolete security roles remain.");
            }

            RecordVersion(
                connection,
                7,
                "1.6.0",
                "Removed obsolete security roles");
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

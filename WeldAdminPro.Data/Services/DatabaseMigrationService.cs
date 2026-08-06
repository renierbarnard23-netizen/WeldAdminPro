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
            if (currentVersion < 8)
            {
                ApplyVersion8(connection);
            }
            if (currentVersion < 9)
            {
                ApplyVersion9(connection);
            }

            if (currentVersion < 10)
            {
                ApplyVersion10(connection);
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
        // VERSION 8
        // General company-wide NCR support
        // =====================================

        private void ApplyVersion8(
            SqliteConnection connection)
        {
            if (!TableExists(
                connection,
                "NcrRecords"))
            {
                throw new InvalidOperationException(
                    "Version 8 migration failed: " +
                    "NcrRecords table does not exist.");
            }

            var originalCount =
                connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM NcrRecords;");

            using var transaction =
                connection.BeginTransaction();

            try
            {
                // =====================================
                // CLEAN FAILED TEMP TABLE IF PRESENT
                // =====================================

                connection.Execute(
                    "DROP TABLE IF EXISTS NcrRecords_V8;",
                    transaction: transaction);

                // =====================================
                // CREATE GENERAL NCR TABLE
                // =====================================
                //
                // WeldId is nullable because a company
                // NCR does not have to concern welding.
                //

                connection.Execute(
                    @"
CREATE TABLE NcrRecords_V8
(
    Id TEXT PRIMARY KEY,

    WeldId TEXT NULL,
    WeldNumber TEXT,

    Description TEXT,
    NcrNumber TEXT,

    RootCause TEXT,
    CorrectiveAction TEXT,
    PreventiveAction TEXT,

    RaisedBy TEXT,
    RaisedDate TEXT,

    AssignedTo TEXT,
    DueDate TEXT,

    Status INTEGER,
    IsClosed INTEGER,

    ClosedBy TEXT,
    ClosedDate TEXT,

    DispositionType INTEGER,
    DispositionApprovedBy TEXT,
    DispositionApprovedDate TEXT,

    VerificationBy TEXT,
    VerificationDate TEXT,

    RequiresCustomerApproval INTEGER DEFAULT 0,
    CustomerApproved INTEGER DEFAULT 0,
    CustomerApprovalReference TEXT,

    Category TEXT,
    CustomReason TEXT,

    IsWeldingRelated INTEGER
        NOT NULL
        DEFAULT 0,

    FOREIGN KEY(WeldId)
        REFERENCES Welds(Id)
);",
                    transaction: transaction);

                // =====================================
                // COPY EXISTING NCR RECORDS
                // =====================================
                //
                // All NCRs in the old design required
                // WeldId, so existing records are
                // classified as welding-related.
                //

                connection.Execute(
                    @"
INSERT INTO NcrRecords_V8
(
    Id,
    WeldId,
    WeldNumber,
    Description,
    NcrNumber,
    RootCause,
    CorrectiveAction,
    PreventiveAction,
    RaisedBy,
    RaisedDate,
    AssignedTo,
    DueDate,
    Status,
    IsClosed,
    ClosedBy,
    ClosedDate,
    DispositionType,
    DispositionApprovedBy,
    DispositionApprovedDate,
    VerificationBy,
    VerificationDate,
    RequiresCustomerApproval,
    CustomerApproved,
    CustomerApprovalReference,
    Category,
    CustomReason,
    IsWeldingRelated
)
SELECT
    Id,
    WeldId,
    WeldNumber,
    Description,
    NcrNumber,
    RootCause,
    CorrectiveAction,
    PreventiveAction,
    RaisedBy,
    RaisedDate,
    AssignedTo,
    DueDate,
    Status,
    IsClosed,
    ClosedBy,
    ClosedDate,
    DispositionType,
    DispositionApprovedBy,
    DispositionApprovedDate,
    VerificationBy,
    VerificationDate,
    RequiresCustomerApproval,
    CustomerApproved,
    CustomerApprovalReference,
    'Welding',
    NULL,
    1
FROM NcrRecords;",
                    transaction: transaction);

                // =====================================
                // VERIFY COPY
                // =====================================

                var copiedCount =
                    connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM NcrRecords_V8;",
                        transaction: transaction);

                if (copiedCount != originalCount)
                {
                    throw new InvalidOperationException(
                        "Version 8 migration failed: " +
                        $"expected {originalCount} NCR records " +
                        $"but copied {copiedCount}.");
                }

                // =====================================
                // REPLACE OLD TABLE
                // =====================================

                connection.Execute(
                    "DROP TABLE NcrRecords;",
                    transaction: transaction);

                connection.Execute(
                    "ALTER TABLE NcrRecords_V8 " +
                    "RENAME TO NcrRecords;",
                    transaction: transaction);

                // =====================================
                // RECREATE WELD LOOKUP INDEX
                // =====================================

                connection.Execute(
                    @"
CREATE INDEX IF NOT EXISTS
IX_NcrRecords_WeldId
ON NcrRecords(WeldId);",
                    transaction: transaction);

                // =====================================
                // FINAL RECORD VALIDATION
                // =====================================

                var finalCount =
                    connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM NcrRecords;",
                        transaction: transaction);

                if (finalCount != originalCount)
                {
                    throw new InvalidOperationException(
                        "Version 8 migration failed: " +
                        $"expected {originalCount} NCR records " +
                        $"after rebuild but found {finalCount}.");
                }

                // =====================================
                // VERIFY NEW SCHEMA
                // =====================================

                var weldIdNotNull =
                    connection.Query(
                        "PRAGMA table_info(NcrRecords);",
                        transaction: transaction)
                    .First(x =>
                        x.name.ToString() == "WeldId")
                    .notnull
                    .ToString();

                if (weldIdNotNull != "0")
                {
                    throw new InvalidOperationException(
                        "Version 8 migration failed: " +
                        "WeldId is still NOT NULL.");
                }

                var requiredColumns =
                    new[]
                    {
                        "Category",
                        "CustomReason",
                        "IsWeldingRelated"
                    };

                var actualColumns =
                    connection.Query(
                        "PRAGMA table_info(NcrRecords);",
                        transaction: transaction)
                    .Select(x =>
                        x.name.ToString())
                    .ToList();

                foreach (var requiredColumn
                    in requiredColumns)
                {
                    if (!actualColumns.Contains(
                        requiredColumn))
                    {
                        throw new InvalidOperationException(
                            "Version 8 migration failed: " +
                            $"column {requiredColumn} is missing.");
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            RecordVersion(
                connection,
                8,
                "1.7.0",
                "Generalized NCR system with optional weld association");
        }

        // =====================================
        // VERSION 9
        // NCR workflow history / audit trail
        // =====================================

        private void ApplyVersion9(
            SqliteConnection connection)
        {
            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS NcrWorkflowHistory
(
    Id TEXT PRIMARY KEY,

    NcrId TEXT NOT NULL,

    FromStatus INTEGER NULL,

    ToStatus INTEGER NOT NULL,

    Action TEXT NOT NULL,

    PerformedBy TEXT,

    PerformedDate TEXT NOT NULL,

    Details TEXT,

    FOREIGN KEY(NcrId)
        REFERENCES NcrRecords(Id)
        ON DELETE CASCADE
);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS IX_NcrWorkflowHistory_NcrId
ON NcrWorkflowHistory(NcrId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS IX_NcrWorkflowHistory_PerformedDate
ON NcrWorkflowHistory(PerformedDate);
");

            RecordVersion(
                connection,
                9,
                "1.8.0",
                "NCR workflow history and audit trail");
        }

                // =====================================
        // VERSION 10
        // NCR to Repair traceability
        // =====================================

        private void ApplyVersion10(
            SqliteConnection connection)
        {
            if (!TableExists(
                connection,
                "RepairRecords"))
            {
                throw new InvalidOperationException(
                    "Version 10 migration failed: " +
                    "RepairRecords table does not exist.");
            }

            TryAddColumn(
                connection,
                "RepairRecords",
                "NcrId",
                "TEXT");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_RepairRecords_NcrId
ON RepairRecords(NcrId);
");

            var ncrIdExists =
                connection.Query(
                    "PRAGMA table_info(RepairRecords);")
                .Any(x =>
                    x.name.ToString() == "NcrId");

            if (!ncrIdExists)
            {
                throw new InvalidOperationException(
                    "Version 10 migration failed: " +
                    "RepairRecords.NcrId was not created.");
            }

            RecordVersion(
                connection,
                10,
                "1.9.0",
                "NCR to Repair traceability");
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


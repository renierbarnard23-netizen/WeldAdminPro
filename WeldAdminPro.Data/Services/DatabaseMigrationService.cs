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

            if (currentVersion < 11)
            {
                ApplyVersion11(connection);
            }

            if (currentVersion < 12)
            {
                ApplyVersion12(connection);
            }

            if (currentVersion < 13)
            {
                ApplyVersion13(connection);
            }

            if (currentVersion < 14)
            {
                ApplyVersion14(connection);
            }
            if (currentVersion < 15)
            {
                ApplyVersion15(connection);
            }

            if (currentVersion < 16)
            {
                ApplyVersion16(connection);
            }

            if (currentVersion < 17)
            {
                ApplyVersion17(connection);
            }


            // =====================================
            if (currentVersion < 18)
            {
                ApplyVersion18(connection);
            }

            // =====================================
            // VERSION 19 - MULTI-COMPANY USER ACCESS
            // =====================================
            if (currentVersion < 19)
            {
                ApplyVersion19(connection);
            }

            // =====================================
            // VERSION 20 - QUALITY MASTER OWNERSHIP
            // =====================================
            if (currentVersion < 20)
            {
                ApplyVersion20(connection);
            }

            // =====================================
            // VERSION 21 - COMPANY DOCUMENT VAULT OWNERSHIP
            // =====================================
            if (currentVersion < 21)
            {
                ApplyVersion21(connection);
            }

            // VERSION 22 - PQR REVISION LINEAGE
            if (currentVersion < 22)
            {
                ApplyVersion22(connection);
            }

            // VERSION 23 - UNIQUE NCR NUMBER PROTECTION
            if (currentVersion < 23)
            {
                ApplyVersion23(connection);
            }

            // VERSION 24 - EXACT REPAIR WPS REVISION TRACEABILITY
            if (currentVersion < 24)
            {
                ApplyVersion24(connection);
            }

            // VERSION 25 - PROJECT WELDING SUBCONTRACTOR OWNERSHIP
            if (currentVersion < 25)
            {
                ApplyVersion25(connection);
            }

            // VERSION 26 - EXACT WELD WPS REVISION TRACEABILITY
            if (currentVersion < 26)
            {
                ApplyVersion26(connection);
            }

            if (currentVersion < 27)
            {
                ApplyVersion27(connection);
            }

            // VERSION 28 - MULTIPLE NDT EQUIPMENT TRACEABILITY
            if (currentVersion < 28)
            {
                ApplyVersion28(connection);
            }

            // VERSION 29 - PROJECT QUALITY CONTROL PLAN
            if (currentVersion < 29)
            {
                ApplyVersion29(connection);
            }

            // VERSION 30 - PROJECT QUALITY CONTROL PLAN ITEMS
            if (currentVersion < 30)
            {
                ApplyVersion30(connection);
            }

            // VERSION 31 - QCP ITEM APPROVAL HISTORY
            if (currentVersion < 31)
            {
                ApplyVersion31(connection);
            }

            // VERSION 32 - QCP MASTER ACTIVITY LIBRARY
            if (currentVersion < 32)
            {
                ApplyVersion32(connection);
            }

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
        // =====================================
        // VERSION 13 - COMPANY COMPLIANCE FOUNDATION

        // =====================================
        // VERSION 14 - COMPANY MASTER DETAILS
        // =====================================

        private void ApplyVersion14(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "Companies",
                "RegistrationNumber",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "VatNumber",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "VatStatus",
                "TEXT NOT NULL DEFAULT 'N/A'");

            TryAddColumn(
                connection,
                "Companies",
                "CompanyType",
                "TEXT NOT NULL DEFAULT 'Other'");

            TryAddColumn(
                connection,
                "Companies",
                "Telephone",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "Mobile",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "Email",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "Website",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "PhysicalAddress",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "PostalAddress",
                "TEXT");

            TryAddColumn(
                connection,
                "Companies",
                "Notes",
                "TEXT");

            RecordVersion(
                connection,
                14,
                "1.13.0",
                "Extended company master details");
        }

        // =====================================


        // =====================================
        // VERSION 15
        // WPQR FOUNDATION
        // =====================================

        private void ApplyVersion15(
            SqliteConnection connection)
        {
            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS Wpqr
(
    Id TEXT NOT NULL PRIMARY KEY,
    WpqrNumber TEXT NOT NULL,

    WpsId TEXT NULL,
    WelderQualificationId INTEGER NULL,

    QualificationDate TEXT NOT NULL,
    RenewalDate TEXT NULL,
    ExpiryDate TEXT NOT NULL,

    IsActive INTEGER NOT NULL DEFAULT 1,

    IsApproved INTEGER NOT NULL DEFAULT 0,
    ApprovedBy TEXT NULL,
    ApprovedOn TEXT NULL,
    ApprovalRole TEXT NULL,

    SignaturePath TEXT NULL,
    StampPath TEXT NULL,

    OriginalDocumentPath TEXT NULL,

    CreatedOn TEXT NOT NULL,
    CreatedBy TEXT NOT NULL
);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS IX_Wpqr_WpsId
ON Wpqr(WpsId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS IX_Wpqr_WelderQualificationId
ON Wpqr(WelderQualificationId);
");

            RecordVersion(
                connection,
                15,
                "1.15.0",
                "WPQR foundation added");
        }
        private void ApplyVersion13(
            SqliteConnection connection)
        {
            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS CompanyContacts
(
    Id TEXT PRIMARY KEY,

    CompanyId TEXT NOT NULL,

    ContactName TEXT NOT NULL,

    Position TEXT,

    Telephone TEXT,

    Mobile TEXT,

    Email TEXT,

    IsPrimary INTEGER NOT NULL DEFAULT 0,

    IsActive INTEGER NOT NULL DEFAULT 1,

    CreatedDate TEXT NOT NULL,

    FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id)
);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyContacts_CompanyId
ON CompanyContacts(CompanyId);
");

            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS CompanyRequirements
(
    Id TEXT PRIMARY KEY,

    CompanyId TEXT NOT NULL,

    RequirementCode TEXT NOT NULL,

    RequirementName TEXT NOT NULL,

    RequirementType TEXT NOT NULL,

    RequirementStatus TEXT NOT NULL,

    Notes TEXT,

    ReviewedDate TEXT,

    ReviewedBy TEXT,

    CreatedDate TEXT NOT NULL,

    FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id)
);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyRequirements_CompanyId
ON CompanyRequirements(CompanyId);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyRequirements_Type
ON CompanyRequirements(RequirementType);
");

            connection.Execute(@"
CREATE TABLE IF NOT EXISTS CompanyDocuments
(
    Id TEXT PRIMARY KEY,

    CompanyId TEXT NOT NULL,

    DocumentType TEXT NOT NULL,

    DocumentName TEXT NOT NULL,

    FileName TEXT NOT NULL,

    FilePath TEXT NOT NULL,

    IssueDate TEXT,

    ExpiryDate TEXT,

    ReviewDate TEXT,

    RequiresRenewal INTEGER NOT NULL DEFAULT 1,

    ReminderDays INTEGER NOT NULL DEFAULT 30,

    IsNA INTEGER NOT NULL DEFAULT 0,

    NAReason TEXT,

    UploadedDate TEXT NOT NULL,

    UploadedBy TEXT,

    IsActive INTEGER NOT NULL DEFAULT 1,

    FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id)
);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyDocuments_CompanyId
ON CompanyDocuments(CompanyId);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyDocuments_ExpiryDate
ON CompanyDocuments(ExpiryDate);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_CompanyDocuments_DocumentType
ON CompanyDocuments(DocumentType);
");

            RecordVersion(
                connection,
                13,
                "1.12.0",
                "Company contacts, compliance requirements and document management foundation");
        }

        // VERSION 10
        // NCR to Repair traceability
        // =====================================
        // VERSION 11
        // Global Repair Reference
        // =====================================

        private void ApplyVersion11(
            SqliteConnection connection)
        {
            if (!TableExists(
                connection,
                "RepairRecords"))
            {
                throw new InvalidOperationException(
                    "Version 11 migration failed: " +
                    "RepairRecords table does not exist.");
            }

            TryAddColumn(
                connection,
                "RepairRecords",
                "RepairReference",
                "TEXT");

            var repairReferenceExists =
                connection.Query(
                    "PRAGMA table_info(RepairRecords);")
                .Any(x =>
                    x.name.ToString() ==
                    "RepairReference");

            if (!repairReferenceExists)
            {
                throw new InvalidOperationException(
                    "Version 11 migration failed: " +
                    "RepairRecords.RepairReference was not created.");
            }

            var existingRepairs =
                connection.Query<dynamic>(
                    @"
SELECT Id
FROM RepairRecords
ORDER BY
    CASE
        WHEN RequestedDate IS NULL
        THEN 1
        ELSE 0
    END,
    RequestedDate,
    Id;
")
                .ToList();

            var repairSequence = 1;

            foreach (var repair in existingRepairs)
            {
                var repairReference =
                    $"REP-{repairSequence:0000}";

                connection.Execute(
                    @"
UPDATE RepairRecords
SET RepairReference = @RepairReference
WHERE Id = @Id
  AND
  (
      RepairReference IS NULL
      OR TRIM(RepairReference) = ''
  );
",
                    new
                    {
                        RepairReference =
                            repairReference,
                        Id =
                            repair.Id.ToString()
                    });

                repairSequence++;
            }

            connection.Execute(
                @"
CREATE UNIQUE INDEX IF NOT EXISTS
IX_RepairRecords_RepairReference
ON RepairRecords(RepairReference);
");

            RecordVersion(
                connection,
                11,
                "1.10.0",
                "Global Repair Reference");
        }

        // =====================================
        // VERSION 12 - COMPANY FOUNDATION
        // =====================================

        private void ApplyVersion12(
            SqliteConnection connection)
        {
            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS Companies
(
    Id TEXT PRIMARY KEY,
    CompanyName TEXT NOT NULL,
    CompanyCode TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    CreatedDate TEXT NOT NULL
)
");

            TryAddColumn(
                connection, "SystemUsers", "CompanyId", "TEXT");

            RecordVersion(
                connection,
                12,
                "1.11.0",
                "Company foundation");
        }
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
        // =====================================
        // VERSION 16 - PROJECT COMPANY ISOLATION
        // =====================================

        private void ApplyVersion16(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "Projects",
                "CompanyId",
                "TEXT");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Projects_CompanyId
ON Projects(CompanyId);
");

            RecordVersion(
                connection,
                16,
                "1.16.0",
                "Project company isolation foundation");
        }
        // SAFE COLUMN ADDER
        // =====================================

        // =====================================
        // VERSION 17 - WPS REVISION LINEAGE
        // =====================================

        private void ApplyVersion17(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "Wps",
                "PreviousRevisionId",
                "TEXT");

            RecordVersion(
                connection,
                17,
                "1.17.0",
                "WPS revision lineage");
        }


        // =====================================
        // VERSION 18 - LEGACY PROJECT OWNERSHIP
        // =====================================

        private void ApplyVersion18(
            SqliteConnection connection)
        {
            var lodexCompanyId =
                connection.ExecuteScalar<string?>(
                    @"
SELECT Id
FROM Companies
WHERE CompanyCode = 'L001'
LIMIT 1;");

            if (string.IsNullOrWhiteSpace(lodexCompanyId))
            {
                throw new InvalidOperationException(
                    "Version 18 migration failed: Lodex company (L001) was not found.");
            }

            connection.Execute(
                @"
UPDATE Projects
SET CompanyId = @CompanyId
WHERE CompanyId IS NULL
  AND ProjectName = 'Blow Pots';",
                new
                {
                    CompanyId = lodexCompanyId
                });

            RecordVersion(
                connection,
                18,
                "1.18.0",
                "Legacy project company ownership migration");
        }

                // =====================================
        // VERSION 19 - MULTI-COMPANY USER ACCESS
        // =====================================

        private void ApplyVersion19(
            SqliteConnection connection)
        {
            // =====================================
            // USER -> COMPANY ACCESS
            // =====================================

            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS UserCompanyAccess
(
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    ValidFrom TEXT NULL,
    ValidUntil TEXT NULL,
    CreatedOn TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    Notes TEXT NULL
);
");

            connection.Execute(
                @"
CREATE UNIQUE INDEX IF NOT EXISTS
IX_UserCompanyAccess_User_Company
ON UserCompanyAccess(UserId, CompanyId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_UserCompanyAccess_UserId
ON UserCompanyAccess(UserId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_UserCompanyAccess_CompanyId
ON UserCompanyAccess(CompanyId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_UserCompanyAccess_IsActive
ON UserCompanyAccess(IsActive);
");

            // =====================================
            // USER -> PROJECT ASSIGNMENT
            // =====================================

            connection.Execute(
                @"
CREATE TABLE IF NOT EXISTS ProjectUserAssignment
(
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    UserId TEXT NOT NULL,
    RoleId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    ValidFrom TEXT NULL,
    ValidUntil TEXT NULL,
    CreatedOn TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    Notes TEXT NULL
);
");

            connection.Execute(
                @"
CREATE UNIQUE INDEX IF NOT EXISTS
IX_ProjectUserAssignment_Project_User
ON ProjectUserAssignment(ProjectId, UserId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_ProjectUserAssignment_ProjectId
ON ProjectUserAssignment(ProjectId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_ProjectUserAssignment_UserId
ON ProjectUserAssignment(UserId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_ProjectUserAssignment_RoleId
ON ProjectUserAssignment(RoleId);
");

            connection.Execute(
                @"
CREATE INDEX IF NOT EXISTS
IX_ProjectUserAssignment_IsActive
ON ProjectUserAssignment(IsActive);
");

            // =====================================
            // PRESERVE EXISTING HOME-COMPANY ACCESS
            // =====================================

            var existingUsers =
                connection.Query(
                    @"
SELECT
    Id,
    CompanyId,
    CreatedDate
FROM SystemUsers
WHERE CompanyId IS NOT NULL;
");

            foreach (var user in existingUsers)
            {
                var userId = user.Id.ToString();
                var companyId = user.CompanyId.ToString();

                var exists =
                    connection.ExecuteScalar<int>(
                        @"
SELECT COUNT(*)
FROM UserCompanyAccess
WHERE UserId = @UserId
  AND CompanyId = @CompanyId;
",
                        new
                        {
                            UserId = userId,
                            CompanyId = companyId
                        });

                if (exists == 0)
                {
                    connection.Execute(
                        @"
INSERT INTO UserCompanyAccess
(
    Id,
    UserId,
    CompanyId,
    IsActive,
    ValidFrom,
    ValidUntil,
    CreatedOn,
    CreatedBy,
    Notes
)
VALUES
(
    @Id,
    @UserId,
    @CompanyId,
    1,
    NULL,
    NULL,
    @CreatedOn,
    'SYSTEM-MIGRATION',
    'Version 19: existing home company access'
);
",
                        new
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            CompanyId = companyId,
                            CreatedOn = user.CreatedDate.ToString()
                        });
                }
            }

            // =====================================
            // MIGRATION VALIDATION
            // =====================================

            var missingHomeAccess =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM SystemUsers u
WHERE u.CompanyId IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM UserCompanyAccess a
      WHERE a.UserId = u.Id
        AND a.CompanyId = u.CompanyId
        AND a.IsActive = 1
  );
");

            if (missingHomeAccess > 0)
            {
                throw new InvalidOperationException(
                    "Version 19 migration failed: " +
                    missingHomeAccess +
                    " existing home-company relationships are missing.");
            }

            // =====================================
            // RECORD VERSION
            // =====================================

            RecordVersion(
                connection,
                19,
                "1.19.0",
                "Multi-company user access and project assignment foundation");
        }

        // =====================================
        // VERSION 20 - QUALITY MASTER OWNERSHIP
        // =====================================

        private void ApplyVersion20(
            SqliteConnection connection)
        {
            // =====================================
            // ADD COMPANY OWNERSHIP COLUMNS
            // =====================================

            TryAddColumn(
                connection,
                "Pqr",
                "CompanyId",
                "TEXT");

            TryAddColumn(
                connection,
                "Wps",
                "CompanyId",
                "TEXT");

            TryAddColumn(
                connection,
                "Wpqr",
                "CompanyId",
                "TEXT");

            TryAddColumn(
                connection,
                "WelderQualification",
                "CompanyId",
                "TEXT");

            // =====================================
            // INDEXES
            // =====================================

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Pqr_CompanyId
ON Pqr(CompanyId);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Wps_CompanyId
ON Wps(CompanyId);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Wpqr_CompanyId
ON Wpqr(CompanyId);
");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_WelderQualification_CompanyId
ON WelderQualification(CompanyId);
");

            // =====================================
            // EXISTING QUALITY MASTER OWNERSHIP
            // Dynamic Options = DYN01
            // =====================================

            var dynamicOptionsCompanyId =
                connection.ExecuteScalar<string?>(
                    @"
SELECT Id
FROM Companies
WHERE CompanyCode = 'DYN01'
  AND IsActive = 1
LIMIT 1;");

            if (string.IsNullOrWhiteSpace(dynamicOptionsCompanyId))
            {
                throw new InvalidOperationException(
                    "Version 20 migration failed: Dynamic Options company (DYN01) was not found.");
            }

            connection.Execute(
                @"
UPDATE Pqr
SET CompanyId = @CompanyId
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
",
                new
                {
                    CompanyId = dynamicOptionsCompanyId
                });

            connection.Execute(
                @"
UPDATE Wps
SET CompanyId = @CompanyId
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
",
                new
                {
                    CompanyId = dynamicOptionsCompanyId
                });

            connection.Execute(
                @"
UPDATE Wpqr
SET CompanyId = @CompanyId
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
",
                new
                {
                    CompanyId = dynamicOptionsCompanyId
                });

            connection.Execute(
                @"
UPDATE WelderQualification
SET CompanyId = @CompanyId
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
",
                new
                {
                    CompanyId = dynamicOptionsCompanyId
                });

            // =====================================
            // MIGRATION VALIDATION
            // =====================================

            var missingPqrCompany =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Pqr
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
");

            var missingWpsCompany =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Wps
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
");

            var missingWpqrCompany =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Wpqr
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
");

            var missingWelderCompany =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM WelderQualification
WHERE CompanyId IS NULL
   OR TRIM(CompanyId) = '';
");

            if (missingPqrCompany > 0 ||
                missingWpsCompany > 0 ||
                missingWpqrCompany > 0 ||
                missingWelderCompany > 0)
            {
                throw new InvalidOperationException(
                    "Version 20 migration failed: one or more quality master records have no CompanyId.");
            }

            // =====================================
            // RELATIONSHIP OWNERSHIP VALIDATION
            // =====================================

            var wpsCompanyMismatch =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Wps w
INNER JOIN Pqr p ON p.Id = w.PqrId
WHERE w.CompanyId <> p.CompanyId;
");

            if (wpsCompanyMismatch > 0)
            {
                throw new InvalidOperationException(
                    "Version 20 migration failed: WPS/PQR company ownership mismatch detected.");
            }

            var wpqrWpsMismatch =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Wpqr q
INNER JOIN Wps w ON w.Id = q.WpsId
WHERE q.CompanyId <> w.CompanyId;
");

            if (wpqrWpsMismatch > 0)
            {
                throw new InvalidOperationException(
                    "Version 20 migration failed: WPQR/WPS company ownership mismatch detected.");
            }

            var wpqrWelderMismatch =
                connection.ExecuteScalar<int>(
                    @"
SELECT COUNT(*)
FROM Wpqr q
INNER JOIN WelderQualification w
    ON w.Id = q.WelderQualificationId
WHERE q.CompanyId <> w.CompanyId;
");

            if (wpqrWelderMismatch > 0)
            {
                throw new InvalidOperationException(
                    "Version 20 migration failed: WPQR/Welder Qualification company ownership mismatch detected.");
            }

            RecordVersion(
                connection,
                20,
                "1.20.0",
                "Quality master company ownership foundation");
        }



        // =====================================
        // VERSION 21
        // =====================================
        private void ApplyVersion21(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "DocumentVaultFiles",
                "CompanyId",
                "TEXT");

            connection.Execute(
                @"CREATE INDEX IF NOT EXISTS IX_DocumentVaultFiles_CompanyId
                  ON DocumentVaultFiles(CompanyId);");

            RecordVersion(
                connection,
                21,
                "1.21.0",
                "Company ownership for Quality Master document vault files");
        }


        // =====================================
        // VERSION 22 - PQR REVISION LINEAGE
        // =====================================
        private void ApplyVersion22(
            SqliteConnection connection)
        {
            TryAddColumn(
                connection,
                "Pqr",
                "PreviousRevisionId",
                "TEXT");

            connection.Execute(
                @"CREATE INDEX IF NOT EXISTS IX_Pqr_PreviousRevisionId
                  ON Pqr(PreviousRevisionId);");

            RecordVersion(
                connection,
                22,
                "1.22.0",
                "PQR revision lineage");
        }

        // =====================================
        // VERSION 23 - UNIQUE NCR NUMBER PROTECTION
        // =====================================
        private void ApplyVersion23(
            SqliteConnection connection)
        {
            connection.Execute(
                @"CREATE UNIQUE INDEX IF NOT EXISTS UX_NcrRecords_NcrNumber
                  ON NcrRecords(NcrNumber)
                  WHERE NcrNumber IS NOT NULL
                    AND TRIM(NcrNumber) <> '';");

            RecordVersion(
                connection,
                23,
                "1.23.0",
                "Unique protection for populated NCR numbers");
        }
        // =====================================
        // VERSION 24 - EXACT REPAIR WPS REVISION TRACEABILITY
        // =====================================
        private void ApplyVersion24(
            SqliteConnection connection)
        {
            if (!TableExists(connection, "RepairRecords"))
            {
                throw new InvalidOperationException(
                    "Version 24 migration failed: RepairRecords table does not exist.");
            }

            TryAddColumn(connection, "RepairRecords", "RepairWpsId", "TEXT");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_RepairRecords_RepairWpsId
ON RepairRecords(RepairWpsId);
");

            var exists = connection.Query(
                    "PRAGMA table_info(RepairRecords);")
                .Any(x => x.name.ToString() == "RepairWpsId");

            if (!exists)
            {
                throw new InvalidOperationException(
                    "Version 24 migration failed: RepairRecords.RepairWpsId was not created.");
            }

            RecordVersion(connection, 24, "1.24.0", "Exact repair WPS revision traceability");
        }

        // =====================================
        // VERSION 25 - PROJECT WELDING SUBCONTRACTOR OWNERSHIP
        // =====================================
        private void ApplyVersion25(
            SqliteConnection connection)
        {
            if (!TableExists(connection, "Projects"))
            {
                throw new InvalidOperationException(
                    "Version 25 migration failed: Projects table does not exist.");
            }

            TryAddColumn(
                connection,
                "Projects",
                "WeldingSubcontractorCompanyId",
                "TEXT");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Projects_WeldingSubcontractorCompanyId
ON Projects(WeldingSubcontractorCompanyId);
");

            var exists = connection.Query(
                    "PRAGMA table_info(Projects);")
                .Any(x => x.name.ToString() == "WeldingSubcontractorCompanyId");

            if (!exists)
            {
                throw new InvalidOperationException(
                    "Version 25 migration failed: Projects.WeldingSubcontractorCompanyId was not created.");
            }

            RecordVersion(
                connection,
                25,
                "1.25.0",
                "Project welding/fabrication subcontractor ownership");
        }

        // =====================================
        // VERSION 26 - EXACT WELD WPS REVISION TRACEABILITY
        // =====================================
        private void ApplyVersion26(
            SqliteConnection connection)
        {
            if (!TableExists(connection, "Welds"))
            {
                throw new InvalidOperationException(
                    "Version 26 migration failed: Welds table does not exist.");
            }

            TryAddColumn(
                connection,
                "Welds",
                "WpsId",
                "TEXT");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_Welds_WpsId
ON Welds(WpsId);
");

            var exists = connection.Query(
                    "PRAGMA table_info(Welds);")
                .Any(x => x.name.ToString() == "WpsId");

            if (!exists)
            {
                throw new InvalidOperationException(
                    "Version 26 migration failed: Welds.WpsId was not created.");
            }

            RecordVersion(
                connection,
                26,
                "1.26.0",
                "Exact weld WPS revision traceability");
        }


        // =====================================
        // VERSION 27 - NDT EQUIPMENT / CALIBRATION TRACEABILITY
        // =====================================
        private void ApplyVersion27(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS Equipment
(
    Id TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    EquipmentNumber TEXT NOT NULL,
    Description TEXT NOT NULL,
    EquipmentType TEXT,
    Manufacturer TEXT,
    Model TEXT,
    SerialNumber TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CalibrationRequired INTEGER NOT NULL DEFAULT 1,
    CalibrationIntervalDays INTEGER NOT NULL DEFAULT 365,
    Notes TEXT,
    CreatedDate TEXT NOT NULL,
    LastModifiedDate TEXT
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_Equipment_Company_Number ON Equipment(CompanyId, EquipmentNumber);
CREATE INDEX IF NOT EXISTS IX_Equipment_CompanyId ON Equipment(CompanyId);

CREATE TABLE IF NOT EXISTS EquipmentCalibrations
(
    Id TEXT PRIMARY KEY,
    EquipmentId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    CertificateNumber TEXT NOT NULL,
    CalibrationDate TEXT NOT NULL,
    ExpiryDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    IsApproved INTEGER NOT NULL DEFAULT 0,
    ApprovedBy TEXT,
    ApprovedDate TEXT,
    Provider TEXT,
    Standard TEXT,
    Notes TEXT,
    DocumentVaultFileId TEXT,
    CreatedDate TEXT NOT NULL,
    FOREIGN KEY(EquipmentId) REFERENCES Equipment(Id)
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_EquipmentCalibrations_Company_Certificate ON EquipmentCalibrations(CompanyId, CertificateNumber);
CREATE INDEX IF NOT EXISTS IX_EquipmentCalibrations_EquipmentId ON EquipmentCalibrations(EquipmentId);
CREATE INDEX IF NOT EXISTS IX_EquipmentCalibrations_ValidRange ON EquipmentCalibrations(EquipmentId, IsActive, IsApproved, CalibrationDate, ExpiryDate);
");

            TryAddColumn(connection, "WeldNdtResults", "EquipmentId", "TEXT");
            TryAddColumn(connection, "WeldNdtResults", "CalibrationRecordId", "TEXT");
            TryAddColumn(connection, "WeldNdtResults", "EquipmentNumberSnapshot", "TEXT");
            TryAddColumn(connection, "WeldNdtResults", "EquipmentSerialNumberSnapshot", "TEXT");
            TryAddColumn(connection, "WeldNdtResults", "CalibrationCertificateSnapshot", "TEXT");

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS IX_WeldNdtResults_EquipmentId ON WeldNdtResults(EquipmentId);
CREATE INDEX IF NOT EXISTS IX_WeldNdtResults_CalibrationRecordId ON WeldNdtResults(CalibrationRecordId);
");

            RecordVersion(connection, 27, "1.27.0", "NDT equipment and calibration traceability");
        }

        // =====================================
        // VERSION 28 - MULTIPLE NDT EQUIPMENT TRACEABILITY
        // =====================================
        private void ApplyVersion28(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS WeldNdtEquipmentUsages
(
    Id TEXT PRIMARY KEY,
    WeldNdtResultId TEXT NOT NULL,
    EquipmentId TEXT NOT NULL,
    CalibrationRecordId TEXT,
    EquipmentNumberSnapshot TEXT NOT NULL,
    EquipmentSerialNumberSnapshot TEXT,
    CalibrationCertificateSnapshot TEXT,
    FOREIGN KEY(WeldNdtResultId) REFERENCES WeldNdtResults(Id) ON DELETE CASCADE,
    FOREIGN KEY(EquipmentId) REFERENCES Equipment(Id)
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_WeldNdtEquipmentUsages_Result_Equipment ON WeldNdtEquipmentUsages(WeldNdtResultId, EquipmentId);
CREATE INDEX IF NOT EXISTS IX_WeldNdtEquipmentUsages_Result ON WeldNdtEquipmentUsages(WeldNdtResultId);
CREATE INDEX IF NOT EXISTS IX_WeldNdtEquipmentUsages_Equipment ON WeldNdtEquipmentUsages(EquipmentId);
");

            RecordVersion(
                connection,
                28,
                "1.28.0",
                "Multiple NDT equipment traceability");
        }

        // =====================================
        // VERSION 29 - PROJECT QUALITY CONTROL PLAN
        // =====================================
        private void ApplyVersion29(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS ProjectQualityControlPlans
(
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    QcpNumber TEXT NOT NULL,
    Revision TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0,
    Title TEXT NOT NULL,
    Description TEXT,
    PreparedBy TEXT,
    PreparedOn TEXT,
    ApprovedBy TEXT,
    ApprovedOn TEXT,
    EffectiveDate TEXT,
    SupersededOn TEXT,
    PreviousRevisionId TEXT,
    IsActive INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(ProjectId)
        REFERENCES Projects(Id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlans_ProjectId
    ON ProjectQualityControlPlans(ProjectId);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlans_Project_QcpNumber
    ON ProjectQualityControlPlans(ProjectId, QcpNumber);

CREATE UNIQUE INDEX IF NOT EXISTS UX_ProjectQualityControlPlans_Project_QcpNumber_Revision
    ON ProjectQualityControlPlans(ProjectId, QcpNumber, Revision);
");

            RecordVersion(
                connection,
                29,
                "1.29.0",
                "Project Quality Control Plan persistence");
        }
        // =====================================
        // VERSION 30 - PROJECT QUALITY CONTROL PLAN ITEMS
        // =====================================
        // =====================================
        // VERSION 31 - QCP ITEM APPROVAL HISTORY
        // =====================================
        private void ApplyVersion31(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS ProjectQualityControlPlanItemApprovalHistory
(
    Id TEXT PRIMARY KEY,
    ProjectQualityControlPlanItemId TEXT NOT NULL,
    Status INTEGER NOT NULL,
    Action TEXT NOT NULL,
    ApprovedBy TEXT,
    ApprovedOn TEXT NOT NULL,
    Remarks TEXT,
    FOREIGN KEY(ProjectQualityControlPlanItemId)
        REFERENCES ProjectQualityControlPlanItems(Id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlanItemApprovalHistory_ItemId
    ON ProjectQualityControlPlanItemApprovalHistory(ProjectQualityControlPlanItemId);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlanItemApprovalHistory_ApprovedOn
    ON ProjectQualityControlPlanItemApprovalHistory(ApprovedOn);
");

            RecordVersion(
                connection,
                31,
                "1.31.0",
                "QCP item approval history persistence");
        }

        private void ApplyVersion30(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS ProjectQualityControlPlanItems
(
    Id TEXT PRIMARY KEY,
    ProjectQualityControlPlanId TEXT NOT NULL,
    SequenceNumber INTEGER NOT NULL,
    Activity TEXT NOT NULL,
    CustomerQualityRequirementId TEXT,
    AcceptanceCriteria TEXT NOT NULL,
    VerificationMethod TEXT NOT NULL,
    ResponsibleParty TEXT NOT NULL,
    InspectionStage TEXT NOT NULL,
    HoldPointCategory INTEGER,
    HoldPointType INTEGER,
    RequiredNdtMethod INTEGER,
    Mandatory INTEGER NOT NULL DEFAULT 0,
    RequiredEvidence TEXT NOT NULL,
    Notes TEXT NOT NULL,
    FOREIGN KEY(ProjectQualityControlPlanId)
        REFERENCES ProjectQualityControlPlans(Id)
        ON DELETE CASCADE,
    FOREIGN KEY(CustomerQualityRequirementId)
        REFERENCES CustomerQualityRequirements(Id)
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlanItems_PlanId
    ON ProjectQualityControlPlanItems(ProjectQualityControlPlanId);

CREATE INDEX IF NOT EXISTS IX_ProjectQualityControlPlanItems_RequirementId
    ON ProjectQualityControlPlanItems(CustomerQualityRequirementId);

CREATE UNIQUE INDEX IF NOT EXISTS UX_ProjectQualityControlPlanItems_Plan_Sequence
    ON ProjectQualityControlPlanItems(ProjectQualityControlPlanId, SequenceNumber);
");

            RecordVersion(
                connection,
                30,
                "1.30.0",
                "Project Quality Control Plan Items persistence");
        }
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
        // =====================================
        // VERSION 32 - QCP MASTER ACTIVITY LIBRARY
        // =====================================
        private void ApplyVersion32(SqliteConnection connection)
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS QcpMasterActivities
(
    Id TEXT PRIMARY KEY,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Category TEXT NOT NULL,
    Description TEXT,
    AcceptanceCriteria TEXT,
    VerificationMethod TEXT,
    ResponsibleParty TEXT,
    InspectionStage TEXT,
    HoldPointCategory INTEGER,
    HoldPointType INTEGER,
    RequiredNdtMethod INTEGER,
    Mandatory INTEGER NOT NULL DEFAULT 0,
    RequiredEvidence TEXT,
    Applicability TEXT,
    ConditionType TEXT,
    SourceReference TEXT,
    Notes TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedBy TEXT,
    CreatedOn TEXT NOT NULL,
    ModifiedBy TEXT,
    ModifiedOn TEXT
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_QcpMasterActivities_Code
    ON QcpMasterActivities(Code);

CREATE INDEX IF NOT EXISTS IX_QcpMasterActivities_Category
    ON QcpMasterActivities(Category);

CREATE INDEX IF NOT EXISTS IX_QcpMasterActivities_IsActive
    ON QcpMasterActivities(IsActive);
");

            RecordVersion(
                connection,
                32,
                "1.32.0",
                "QCP Master Activity Library persistence");
        }
    }
}










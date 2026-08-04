using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var connection = 
                new SqliteConnection(
                    $"Data Source={DatabasePath.Get()}");

            connection.Open();

            CreateReservedMaterialsTable(connection);

            using var cmd = 
                connection.CreateCommand();

            // Enable foreign keys
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

            connection.Execute(
    @"
CREATE TABLE IF NOT EXISTS DatabaseVersions
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SchemaVersion INTEGER NOT NULL,
    BuildVersion TEXT NOT NULL,
    AppliedDate TEXT NOT NULL,
    Notes TEXT
)");

            connection.Execute(@"
INSERT INTO DatabaseVersions
(
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
)
SELECT
    1,
    '1.0.0',
    datetime('now'),
    'Initial Production Schema'
WHERE NOT EXISTS
(
    SELECT 1
    FROM DatabaseVersions
);
");

            connection.Execute(
    @"
CREATE TABLE IF NOT EXISTS AuditTrailEntries
(
    Id TEXT PRIMARY KEY,
    Timestamp TEXT NOT NULL,
    UserName TEXT,
    Module TEXT,
    Action TEXT,
    EntityType TEXT,
    EntityId TEXT,
    Details TEXT
)");

connection.Execute(
    @"
CREATE TABLE IF NOT EXISTS SystemUsers
(
    Id TEXT PRIMARY KEY,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Email TEXT,
    Role INTEGER NOT NULL,
    IsActive INTEGER NOT NULL,
    CreatedDate TEXT NOT NULL,
    LastLoginDate TEXT
)");

            AddColumnIfNotExists(connection, "SystemUsers", "Email", "TEXT");
            AddColumnIfNotExists(connection, "SystemUsers", "LastLoginDate", "TEXT");

            connection.Execute(
    @"
CREATE TABLE IF NOT EXISTS AuditLogs
(
    Id TEXT PRIMARY KEY,

    Username TEXT,

    Action TEXT,

    Module TEXT,

    Details TEXT,

    Timestamp TEXT
)");

            


            // =========================
            // PQR TABLE
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Pqr (
    Id TEXT PRIMARY KEY,
    PqrNumber TEXT,
    QualificationDate TEXT,
    QualifiedBy TEXT,
    ThicknessTested REAL,

    Process TEXT,
    MaterialGroup TEXT,
    Position TEXT,

    FillerMaterial TEXT,
    GasType TEXT,

    AmpsUsed REAL,
    VoltsUsed REAL,
    HeatInput REAL,

    Preheat REAL,
    Interpass REAL,
    PwhtPerformed INTEGER,

    WpsId TEXT
);";
            cmd.ExecuteNonQuery();

            // PQR ISO fields
            AddColumnIfNotExists(connection, "Pqr", "PNumber", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "FNumber", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "QualifiedPosition", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "JointType", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "ThicknessQualifiedMin", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "ThicknessQualifiedMax", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "DiameterMin", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "DiameterMax", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "WpsReferenceNumber", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "IsApproved", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Pqr", "ApprovedOn", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "ApprovedBy", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "IsLocked", "INTEGER DEFAULT 0");

            AddColumnIfNotExists(connection, "Pqr", "Revision", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Pqr", "IsActive", "INTEGER DEFAULT 1");

            AddColumnIfNotExists(connection, "Pqr", "Standard", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "JointDesign", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "SurfacePreparation", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "GrooveAngle", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "RootFace", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "RootGap", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "Backing", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "BackGouging", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "JointDiagramPath", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "PassDiagramPath", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "GrooveRadius", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "Misalignment", "REAL");
            AddColumnIfNotExists(connection, "Pqr", "BackingType", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "EdgePreparation", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "BaseMaterial1", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "BaseMaterial2", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "BaseMaterial1Specification", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "BaseMaterial2Specification", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "FillerClassification", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "ANumber", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "WeldingPosition", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "WeldingType", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "Progression", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "CurrentType", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "TransferMode", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "Polarity", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "TravelSpeed", "REAL");

            AddColumnIfNotExists(connection, "Pqr", "ShieldingGas", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "GasFlowRate", "REAL");

            AddColumnIfNotExists(connection, "Pqr", "BackingGas", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "BackingGasFlowRate", "REAL");

            AddColumnIfNotExists(connection, "Pqr", "PreheatNotes", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "PreheatTemperature", "TEXT");
            AddColumnIfNotExists(connection, "Pqr", "PostWeldHeatTreatment", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "QualifiedPNumberRange", "TEXT");

            AddColumnIfNotExists(connection, "Pqr", "Revision", "INTEGER");
            AddColumnIfNotExists(connection, "Pqr", "IsActive", "INTEGER");
            AddColumnIfNotExists(connection, "Pqr", "IsPipe", "INTEGER");

            AddColumnIfNotExists(connection, "Pqr", "PNumber2", "TEXT");

            // =========================
            // WPS TABLE (CRITICAL FIX)
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS WpsRecords (
    Id TEXT PRIMARY KEY,
    WpsNumber TEXT NOT NULL,
    Revision INTEGER NOT NULL,

    PqrId TEXT NOT NULL,
    PqrNumber TEXT,

    ThicknessMin REAL,
    ThicknessMax REAL,

    DiameterMin REAL,
    DiameterMax REAL,

    PositionRange TEXT,

    PNumber INTEGER,
    FNumber INTEGER,

    IsApproved INTEGER NOT NULL,
    IsLocked INTEGER NOT NULL
);";
            cmd.ExecuteNonQuery();

            AddColumnIfNotExists(connection, "WpsRecords", "IsApproved", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WpsRecords", "IsLocked", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WpsRecords", "ApprovedOn", "TEXT");
            AddColumnIfNotExists(connection, "WpsRecords", "ApprovedBy", "TEXT");

            AddColumnIfNotExists(connection, "WpsRecords", "Revision", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WpsRecords", "IsActive", "INTEGER DEFAULT 1");

            // =========================
            // PROJECTS
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Projects (
    Id TEXT PRIMARY KEY,
    JobNumber INTEGER NOT NULL UNIQUE,
    ProjectName TEXT NOT NULL,
    Client TEXT NOT NULL,
    ClientRepresentative TEXT,
    Amount REAL NOT NULL DEFAULT 0,
    Budget REAL NOT NULL DEFAULT 0,
    QuoteNumber TEXT,
    OrderNumber TEXT,
    Material TEXT,
    AssignedTo TEXT,
    IsInvoiced INTEGER NOT NULL,
    InvoiceNumber TEXT,
    StartDate TEXT,
    EndDate TEXT,
    Status INTEGER NOT NULL,
    CreatedOn TEXT NOT NULL,
    ActualCost REAL NOT NULL DEFAULT 0,
    CommittedCost REAL NOT NULL DEFAULT 0,
    CompletedOn TEXT,
    LastModifiedOn TEXT,
    IsArchived INTEGER NOT NULL DEFAULT 0
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_Projects_JobNumber
                ON Projects(JobNumber);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_Projects_Status
                ON Projects(Status);");

            // =========================
            // STOCK ITEMS
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StockItems (
    Id TEXT PRIMARY KEY,
    ItemCode TEXT NOT NULL,
    Description TEXT,
    Quantity INTEGER NOT NULL,
    Unit TEXT,
    MinLevel REAL,
    MaxLevel REAL,
    Category TEXT NOT NULL DEFAULT 'Uncategorised',
    AverageUnitCost REAL NOT NULL DEFAULT 0
);";
            cmd.ExecuteNonQuery();

            // =========================
            // STOCK TRANSACTIONS
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StockTransactions (
    Id TEXT PRIMARY KEY,
    StockItemId TEXT NOT NULL,
    ProjectId TEXT NULL,
    TransactionDate TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    Type TEXT NOT NULL,
    UnitCost REAL NOT NULL DEFAULT 0,
    Reference TEXT,
    BalanceAfter INTEGER NULL,

    FOREIGN KEY (StockItemId)
        REFERENCES StockItems(Id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    FOREIGN KEY (ProjectId)
        REFERENCES Projects(Id)
        ON DELETE SET NULL
);";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_StockTransactions_StockItemId
                ON StockTransactions(StockItemId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_StockTransactions_ProjectId
                ON StockTransactions(ProjectId);");

            AddColumnIfNotExists(connection, "StockTransactions", "BalanceAfter", "INTEGER");

            // =========================
            // PROJECT DOCUMENTS TABLE
            // =========================

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ProjectDocuments
                (
                    Id TEXT PRIMARY KEY,
                    ProjectId TEXT,
                    DocumentType TEXT,
                    IsRequired INTEGER,
                    IsUploaded INTEGER,
                    FilePath TEXT,
                    UploadedDate TEXT,

                FOREIGN KEY(ProjectId)
                    REFERENCES Projects(Id)
                    ON DELETE CASCADE
                );";

            cmd.ExecuteNonQuery();

            // Ensure columns exist (for old DBs)
            AddColumnIfNotExists(connection, "ProjectDocuments", "IsRequired", "INTEGER");
            AddColumnIfNotExists(connection, "ProjectDocuments", "IsUploaded", "INTEGER");
            AddColumnIfNotExists(connection, "ProjectDocuments", "FilePath", "TEXT");
            AddColumnIfNotExists(connection, "ProjectDocuments", "UploadedDate", "TEXT");
            AddColumnIfNotExists(connection, "ProjectDocuments", "DocumentType", "TEXT DEFAULT 'Unknown'");

            AddColumnIfNotExists(connection,
    "ProjectDocuments",
    "DocumentName",
    "TEXT");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "Revision",
                "INTEGER DEFAULT 0");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "IsApproved",
                "INTEGER DEFAULT 0");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "IsLocked",
                "INTEGER DEFAULT 0");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "AllowMultiple",
                "INTEGER DEFAULT 0");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "Category",
                "TEXT");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "CreatedOn",
                "TEXT");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "ApprovedOn",
                "TEXT");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "ApprovedBy",
                "TEXT");

            AddColumnIfNotExists(connection,
                "ProjectDocuments",
                "LastModifiedOn",
                "TEXT");

            // =========================
            // WELD HISTORY
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS WeldHistory
(
    Id TEXT PRIMARY KEY,
    WeldId TEXT NOT NULL,
    EventDate TEXT NOT NULL,
    EventType TEXT NOT NULL,
    Description TEXT,
    UserName TEXT,
    StatusSnapshot TEXT
);";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_WeldHistory_WeldId
                ON WeldHistory(WeldId);");

            // =========================
            // PURCHASE ORDERS
            // =========================

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS PurchaseOrders (
                    Id TEXT PRIMARY KEY,
                    ProjectId TEXT NOT NULL,
                    JobNumber INTEGER,
                    PONumber TEXT NOT NULL,
                    SupplierName TEXT,
                    CreatedDate TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    TotalAmount REAL NOT NULL,

                FOREIGN KEY(ProjectId)
                    REFERENCES Projects(Id)
                    ON DELETE CASCADE
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_PurchaseOrders_ProjectId
                ON PurchaseOrders(ProjectId);");

            // =========================
            // PURCHASE ORDER LINES
            // =========================

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS PurchaseOrderLines (
                    Id TEXT PRIMARY KEY,
                    PurchaseOrderId TEXT NOT NULL,
                    StockItemId TEXT NOT NULL,
                    ItemCode TEXT,
                    Description TEXT,
                    Quantity INTEGER NOT NULL,
                    UnitCost REAL NOT NULL,
                    LineTotal REAL NOT NULL,

                FOREIGN KEY(PurchaseOrderId)
                    REFERENCES PurchaseOrders(Id)
                    ON DELETE CASCADE,

                FOREIGN KEY(StockItemId)
                    REFERENCES StockItems(Id)
                    ON DELETE RESTRICT
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_PurchaseOrderLines_PurchaseOrderId
                ON PurchaseOrderLines(PurchaseOrderId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_PurchaseOrderLines_StockItemId
                ON PurchaseOrderLines(StockItemId);");

            // =========================
            // CATEGORIES
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Categories (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS ProjectStockUsages (
        Id TEXT PRIMARY KEY,
        ProjectId TEXT NOT NULL,
        StockItemId TEXT NOT NULL,
        Quantity REAL NOT NULL,
        UnitCostAtIssue REAL NOT NULL,
        IssuedOn TEXT NOT NULL,
        IssuedBy TEXT,
        Notes TEXT,

    FOREIGN KEY(ProjectId)
        REFERENCES Projects(Id)
        ON DELETE CASCADE,

    FOREIGN KEY(StockItemId)
        REFERENCES StockItems(Id)
        ON DELETE RESTRICT
);";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_ProjectStockUsages_ProjectId
                ON ProjectStockUsages(ProjectId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_ProjectStockUsages_StockItemId
                ON ProjectStockUsages(StockItemId);");

            // =========================
            // AUDIT LOG
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS AuditLog (
    Id TEXT PRIMARY KEY,
    ActionType TEXT NOT NULL,
    Description TEXT NOT NULL,
    EntityId TEXT,
    Username TEXT,
    MachineName TEXT,
    Severity TEXT NOT NULL,
    Timestamp TEXT NOT NULL
);";
            cmd.ExecuteNonQuery();

            // =========================
            // WELDER QUALIFICATION TABLE (ISO CRITICAL)
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS WelderQualification (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    WelderNumber TEXT NOT NULL,

    Process TEXT NOT NULL,

    MaterialGroup TEXT,

    Position TEXT NOT NULL,

    ThicknessMin REAL,

    ThicknessMax REAL,

    QualificationDate TEXT NOT NULL,

    InitialQualificationDate TEXT,

    RenewalDate TEXT,

    ExpiryDate TEXT NOT NULL
);";
            cmd.ExecuteNonQuery();

            AddColumnIfNotExists(connection, "WelderQualification", "MaterialGroup", "TEXT");
            AddColumnIfNotExists(connection, "WelderQualification", "ThicknessMin", "REAL");
            AddColumnIfNotExists(connection, "WelderQualification", "ThicknessMax", "REAL");
            AddColumnIfNotExists(connection, "WelderQualification", "InitialQualificationDate", "TEXT");
            AddColumnIfNotExists(connection, "WelderQualification", "RenewalDate", "TEXT");

            // =========================
            // WELD NDT RESULTS
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS WeldNdtResults
(
    Id TEXT PRIMARY KEY,
    WeldId TEXT NOT NULL,
    NdtMethod TEXT NOT NULL,
    Result TEXT NOT NULL,
    Notes TEXT,
    ReportNumber TEXT,
    Inspector TEXT,
    InspectionDate TEXT,
    InspectorName TEXT,
    AcceptanceCriteria TEXT,
    Remarks TEXT,
    RequiresRepair INTEGER NOT NULL DEFAULT 0,
    RepairCycle INTEGER NOT NULL DEFAULT 0,
    IsReinspection INTEGER NOT NULL DEFAULT 0,

    FOREIGN KEY(WeldId)
        REFERENCES Welds(Id)
        ON DELETE CASCADE
);
";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
CREATE INDEX IF NOT EXISTS
IX_WeldNdtResults_WeldId
ON WeldNdtResults(WeldId);");

            AddColumnIfNotExists(connection, "WeldNdtResults", "AcceptanceCriteria", "TEXT");
            AddColumnIfNotExists(connection, "WeldNdtResults", "RequiresRepair", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WeldNdtResults", "RepairCycle", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WeldNdtResults", "IsReinspection", "INTEGER DEFAULT 0");

            // =========================
            // WELD TABLE
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Welds
(
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,

    WeldNumber TEXT,
    DrawingNumber TEXT,
    JointType TEXT,

    WpsNumber TEXT,
    WelderNumber TEXT,

    Process TEXT,
    MaterialGroup TEXT,
    Position TEXT,
    Thickness REAL,

    MaterialHeat1 TEXT,
    MaterialHeat2 TEXT,

    Status TEXT,
    WorkflowStatus TEXT,
    NdtStatus TEXT,

    RepairCount INTEGER,
    RepairCycle INTEGER,
    RequiresRepair INTEGER,

    LastNdtDate TEXT,
    LastNdtResult TEXT,
    
    NdtPendingDate TEXT,
    ReleaseReady INTEGER DEFAULT 0,
    TurnoverReady INTEGER DEFAULT 0,
    BlockingCount INTEGER DEFAULT 0,
    ReadinessSummary TEXT,
    ReleasedBy TEXT,
    ReleasedDate TEXT,
    IsReleased INTEGER DEFAULT 0,

    RequiredReleaseRole INTEGER DEFAULT 0,

    IsValid INTEGER,
    ValidationMessage TEXT,

    CreatedDate TEXT,

    FOREIGN KEY(ProjectId)
        REFERENCES Projects(Id)
        ON DELETE CASCADE

);
";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_Welds_ProjectId
                ON Welds(ProjectId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_Welds_WeldNumber
                ON Welds(WeldNumber);");

            AddColumnIfNotExists(connection, "Welds", "Process", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "MaterialGroup", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "Position", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "Thickness", "REAL");

            AddColumnIfNotExists(connection, "Welds", "IsValid", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Welds", "ValidationMessage", "TEXT");

            AddColumnIfNotExists(connection, "Welds", "WorkflowStatus", "TEXT");

            AddColumnIfNotExists(connection, "Welds", "NdtPendingDate", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "ReleaseReady", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Welds", "TurnoverReady", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Welds", "BlockingCount", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Welds", "ReadinessSummary", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "ReleasedBy", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "ReleasedDate", "TEXT");
            AddColumnIfNotExists(connection, "Welds", "IsReleased", "INTEGER DEFAULT 0");

            cmd.CommandText = @"

CREATE TABLE IF NOT EXISTS ProjectDocumentFiles
(
    Id TEXT PRIMARY KEY,
    ProjectDocumentId TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    FileName TEXT NOT NULL,
    UploadedOn TEXT NOT NULL,
    IsApproved INTEGER NOT NULL,

    FOREIGN KEY(ProjectDocumentId)
        REFERENCES ProjectDocuments(Id)
        ON DELETE CASCADE
);
";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_ProjectDocuments_ProjectId
                ON ProjectDocuments(ProjectId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_ProjectDocumentFiles_ProjectDocumentId
                ON ProjectDocumentFiles(ProjectDocumentId);");

            // =========================
            // HOLD POINTS
            // =========================
            cmd.CommandText = @"

            CREATE TABLE IF NOT EXISTS WeldHoldPoints
(
    Id TEXT PRIMARY KEY,
    WeldId TEXT NOT NULL,
    HoldPointType INTEGER NOT NULL,
    Category INTEGER,
    RequiredApproverRole INTEGER,
    Status INTEGER NOT NULL,
    IsMandatory INTEGER NOT NULL,
    ApprovedBy TEXT,
    ApprovedDate TEXT,
    Comments TEXT,

    FOREIGN KEY(WeldId)
    REFERENCES Welds(Id)
    ON DELETE CASCADE
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_WeldHoldPoints_WeldId
                ON WeldHoldPoints(WeldId);");

            AddColumnIfNotExists(connection, "WeldHoldPoints", "Category", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "WeldHoldPoints", "RequiredApproverRole", "INTEGER DEFAULT 0");
            
            AddColumnIfNotExists(connection, "Welds", "WeldType", "TEXT");

            // =========================
            // REPAIR TABLE
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS RepairRecords
(
Id TEXT PRIMARY KEY,

WeldId TEXT NOT NULL,

RepairNumber INTEGER NOT NULL,

Reason TEXT,

AuthorizedBy TEXT,

RequestedDate TEXT,

AuthorizedDate TEXT,

ExcavationMethod TEXT,

RepairWpsNumber TEXT,

RepairedByWelder TEXT,

ReinspectionResult TEXT,

Notes TEXT,

Status INTEGER,

FOREIGN KEY(WeldId)
    REFERENCES Welds(Id)
    ON DELETE CASCADE
);
";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_RepairRecords_WeldId
                ON RepairRecords(WeldId);");

            // =========================
            // NCR RECORDS TABLE
            // =========================

            cmd.CommandText =
@"CREATE TABLE IF NOT EXISTS NcrRecords
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
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_NcrRecords_WeldId
                ON NcrRecords(WeldId);");

            AddColumnIfNotExists(connection, "NcrRecords", "RootCause", "TEXT");

            AddColumnIfNotExists(connection, "NcrRecords", "CorrectiveAction", "TEXT");

            AddColumnIfNotExists(connection, "NcrRecords", "PreventiveAction", "TEXT");

            AddColumnIfNotExists(connection, "NcrRecords", "AssignedTo", "TEXT");

            AddColumnIfNotExists(connection, "NcrRecords", "DueDate", "TEXT");

            AddColumnIfNotExists(connection, "NcrRecords", "Status", "INTEGER");

            // =========================
            // CAPA RECORDS TABLE
            // =========================

            connection.Execute(
@"
CREATE TABLE IF NOT EXISTS CapaRecords
(
    Id TEXT PRIMARY KEY,

    CapaNumber TEXT,

    NcrId TEXT,

    Title TEXT,

    RootCause TEXT,

    CorrectiveAction TEXT,

    PreventiveAction TEXT,

    AssignedTo TEXT,

    DueDate TEXT,

    CreatedDate TEXT,

    CompletedDate TEXT,

    CreatedBy TEXT,

    VerifiedBy TEXT,

    VerifiedDate TEXT,

    IsEffective INTEGER,

    Priority INTEGER,

    Status INTEGER
)");

            // =========================
            // QCP INSPECTION RULES TABLE
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS QcpInspectionRules
(
    Id TEXT PRIMARY KEY,

    WeldType TEXT,

    RequiredNdtType INTEGER,

    InspectionPercentage REAL,

    RequiresClientWitness INTEGER,

    RequiresHoldPoint INTEGER
);
";

            cmd.ExecuteNonQuery();

            // =========================
            // WELD TRACEABILITY TABLE
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS WeldTraceability
(
    Id TEXT PRIMARY KEY,

    WeldId TEXT,

    WpsNumber TEXT,

    PqrNumber TEXT,

    WelderQualification TEXT,

    MaterialHeatNumber TEXT,

    ConsumableBatch TEXT,

    NdtReportNumber TEXT,

    ReleaseCertificate TEXT
);
";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
            CREATE INDEX IF NOT EXISTS
                IX_WeldTraceability_WeldId
                ON WeldTraceability(WeldId);");

            // =========================
            // TURNOVER PACKAGES TABLE
            // =========================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS TurnoverPackages
(
    Id TEXT PRIMARY KEY,

    ProjectId TEXT,

    PackageNumber TEXT,

    CreatedDate TEXT,

    CreatedBy TEXT,

    IsApproved INTEGER,

    ApprovedBy TEXT,

    ApprovedDate TEXT
);";
            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_TurnoverPackages_ProjectId
                ON TurnoverPackages(ProjectId);");

            // =====================================
            // DOCUMENT VAULT
            // =====================================

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS DocumentVaultFiles
(
    Id TEXT PRIMARY KEY,

    FileName TEXT NOT NULL,

    OriginalFileName TEXT NOT NULL,

    FilePath TEXT NOT NULL,

    Category INTEGER NOT NULL,

    Description TEXT,

    DocumentNumber TEXT,
    
    Title TEXT,
    
    Status TEXT,

    Revision TEXT,

    UploadedDate TEXT NOT NULL,

    UploadedBy TEXT,

    WeldId TEXT NULL,

    ProjectId TEXT NULL,

    IsApproved INTEGER NOT NULL    

);";

            cmd.ExecuteNonQuery();

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_DocumentVaultFiles_ProjectId
                ON DocumentVaultFiles(ProjectId);");

            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS
                IX_DocumentVaultFiles_WeldId
                ON DocumentVaultFiles(WeldId);");

            
        }

        // =========================
        // SAFE COLUMN ADDER
        // =========================
        private static void AddColumnIfNotExists(SqliteConnection connection, string table, string column, string type)
        {
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"PRAGMA table_info({table});";

            using var reader = checkCmd.ExecuteReader();

            while (reader.Read())
            {
                var existingColumn = reader["name"].ToString();
                if (string.Equals(existingColumn, column, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type};";
            alterCmd.ExecuteNonQuery();
        }        

        private static void CreateReservedMaterialsTable(
    SqliteConnection connection)
        {
            using var cmd =
                connection.CreateCommand();

            cmd.CommandText =
            @"
    CREATE TABLE IF NOT EXISTS ReservedMaterials
    (
        Id TEXT PRIMARY KEY,
        WorkOrderId TEXT NOT NULL,
        ItemCode TEXT NOT NULL,
        Quantity REAL NOT NULL,
        ReservedOn TEXT NOT NULL
    );
    ";

            cmd.ExecuteNonQuery();
        }

    }
}
using Microsoft.Data.Sqlite;

namespace WeldAdminPro.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
            connection.Open();

            using var cmd = connection.CreateCommand();

            // Enable foreign keys
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

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
        ON UPDATE CASCADE
);";
            cmd.ExecuteNonQuery();

            AddColumnIfNotExists(connection, "StockTransactions", "BalanceAfter", "INTEGER");

            


            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ProjectDocuments (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT,
    FileName TEXT,
    FilePath TEXT,
    UploadedOn TEXT
);";
            cmd.ExecuteNonQuery();

            // =========================
            // PROJECT DOCUMENTS TABLE
            // =========================
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ProjectDocuments (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT,
    DocumentType TEXT,
    IsRequired INTEGER,
    IsUploaded INTEGER,
    FilePath TEXT,
    UploadedDate TEXT
);";
            cmd.ExecuteNonQuery();

            // Ensure columns exist (for old DBs)
            AddColumnIfNotExists(connection, "ProjectDocuments", "DocumentType", "TEXT");
            AddColumnIfNotExists(connection, "ProjectDocuments", "IsRequired", "INTEGER");
            AddColumnIfNotExists(connection, "ProjectDocuments", "IsUploaded", "INTEGER");
            AddColumnIfNotExists(connection, "ProjectDocuments", "FilePath", "TEXT");
            AddColumnIfNotExists(connection, "ProjectDocuments", "UploadedDate", "TEXT");
            AddColumnIfNotExists(connection, "ProjectDocuments", "DocumentType", "TEXT DEFAULT 'Unknown'");

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
    TotalAmount REAL NOT NULL
);";
            cmd.ExecuteNonQuery();

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
    FOREIGN KEY (PurchaseOrderId)
        REFERENCES PurchaseOrders(Id)
        ON DELETE CASCADE
);";
            cmd.ExecuteNonQuery();

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
    Notes TEXT
);";
            cmd.ExecuteNonQuery();

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
    WelderName TEXT NOT NULL,
    Process TEXT NOT NULL,
    Position TEXT NOT NULL,
    QualificationDate TEXT NOT NULL,
    ExpiryDate TEXT NOT NULL
);";
            cmd.ExecuteNonQuery();
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
    }
}
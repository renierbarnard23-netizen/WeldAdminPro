using System.Net.NetworkInformation;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Migrations;

namespace WeldAdminPro.Data
{
	public static class DatabaseInitializer
	{
		public static void Initialize()
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "PRAGMA foreign_keys = ON;";
			cmd.ExecuteNonQuery();

			// ===============================
			// STOCK ITEMS
			// ===============================
			cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS StockItems (
        Id TEXT PRIMARY KEY,
        ItemCode TEXT NOT NULL,
        Description TEXT,
        Quantity INTEGER NOT NULL,
        Unit TEXT,
        MinLevel REAL NULL,
        MaxLevel REAL NULL,
        Category TEXT NOT NULL DEFAULT 'Uncategorised',
        AverageUnitCost REAL NOT NULL DEFAULT 0
    );";
			cmd.ExecuteNonQuery();

			// ===============================
			// STOCK TRANSACTIONS
			// ===============================
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

			// ===============================
			// PROJECT STOCK USAGES
			// ===============================
			cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS ProjectStockUsages (
        Id TEXT PRIMARY KEY,
        ProjectId TEXT NOT NULL,
        StockItemId TEXT NOT NULL,
        Quantity REAL NOT NULL,
        UnitCostAtIssue REAL NOT NULL DEFAULT 0,
        IssuedOn TEXT NOT NULL,
        IssuedBy TEXT,
        Notes TEXT
        );";
			cmd.ExecuteNonQuery();

			// ===============================
			// PROJECTS
			// ===============================
			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Projects (
    Id TEXT PRIMARY KEY,
    JobNumber INTEGER NOT NULL,
    ProjectName TEXT NOT NULL,
    Client TEXT,
    ClientRepresentative TEXT,
    Amount REAL NOT NULL DEFAULT 0,
    Budget REAL NOT NULL DEFAULT 0,
    QuoteNumber TEXT,
    OrderNumber TEXT,
    Material TEXT,
    AssignedTo TEXT,
    IsInvoiced INTEGER NOT NULL DEFAULT 0,
    InvoiceNumber TEXT,
    StartDate TEXT,
    EndDate TEXT,
    Status TEXT NOT NULL,
    CreatedOn TEXT NOT NULL,
    ActualCost REAL NOT NULL DEFAULT 0,
    CommittedCost REAL NOT NULL DEFAULT 0,
    CompletedOn TEXT NULL,
    LastModifiedOn TEXT NULL,
    IsArchived INTEGER NOT NULL DEFAULT 0
);";
			cmd.ExecuteNonQuery();

			// ===============================
			// PURCHASE ORDERS
			// ===============================
			cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS PurchaseOrders (
        Id TEXT PRIMARY KEY,
        ProjectId TEXT NOT NULL,
        JobNumber INTEGER NOT NULL,
        PONumber TEXT NOT NULL,
        SupplierName TEXT,
        CreatedDate TEXT NOT NULL,
        Status TEXT NOT NULL,
        TotalAmount REAL NOT NULL
    );";
			cmd.ExecuteNonQuery();

			// ===============================
			// PURCHASE ORDER LINES
			// ===============================
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

			// ===============================
			// CATEGORIES
			// ===============================
			cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS Categories (
        Id TEXT PRIMARY KEY,
        Name TEXT NOT NULL,
        IsActive INTEGER NOT NULL DEFAULT 1
    );";
			cmd.ExecuteNonQuery();

			// ===============================
			// AUDIT LOG
			// ===============================
			cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS AuditLog (
        Id TEXT PRIMARY KEY,
        ActionType TEXT NOT NULL,
        Description TEXT NOT NULL,
        EntityId TEXT NULL,
        Username TEXT NULL,
        MachineName TEXT NULL,
        Severity TEXT NOT NULL,
        Timestamp TEXT NOT NULL
    );";
			cmd.ExecuteNonQuery();
		}
	}
}
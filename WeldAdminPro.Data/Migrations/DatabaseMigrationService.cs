using System;
using Microsoft.Data.Sqlite;

namespace WeldAdminPro.Data.Migrations
{
	public class DatabaseMigrationService
	{
		private readonly string _connectionString;

		public DatabaseMigrationService(string connectionString)
		{
			_connectionString = connectionString;
		}
		private bool ColumnExists(SqliteConnection conn, string table, string column)
		{
			using var cmd = conn.CreateCommand();

			cmd.CommandText = $"PRAGMA table_info({table});";

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				if (reader.GetString(1).Equals(column, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		public void RunMigrations()
		{
			using var conn = new SqliteConnection(_connectionString);
			conn.Open();

			EnsureVersionTable(conn);

			int currentVersion = GetCurrentVersion(conn);

			if (currentVersion < 1)
				Migration1_CreateBaseTables(conn);

			if (currentVersion < 2)
				Migration2_AddAnalyticsColumns(conn);

			if (currentVersion < 3)
				Migration3_AddBarcodeFields(conn);

			if (currentVersion < 4)
				Migration4_AddLocations(conn);
		}

		private void EnsureVersionTable(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();

			cmd.CommandText =
			@"CREATE TABLE IF NOT EXISTS DatabaseVersion(
                Version INTEGER NOT NULL
            );";

			cmd.ExecuteNonQuery();

			cmd.CommandText =
			@"INSERT INTO DatabaseVersion (Version)
              SELECT 0
              WHERE NOT EXISTS (SELECT 1 FROM DatabaseVersion);";

			cmd.ExecuteNonQuery();
		}

		private int GetCurrentVersion(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT Version FROM DatabaseVersion LIMIT 1";

			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		private void SetVersion(SqliteConnection conn, int version)
		{
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "UPDATE DatabaseVersion SET Version=@v";
			cmd.Parameters.AddWithValue("@v", version);
			cmd.ExecuteNonQuery();
		}

		// ----------------------------
		// MIGRATION 1
		// ----------------------------

		private void Migration1_CreateBaseTables(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();

			cmd.CommandText =
			@"CREATE TABLE IF NOT EXISTS StockItems(
                Id TEXT PRIMARY KEY,
                ItemCode TEXT,
                Description TEXT,
                Quantity INTEGER,
                Unit TEXT,
                MinLevel REAL,
                MaxLevel REAL,
                AverageUnitCost REAL,
                Category TEXT
            );";

			cmd.ExecuteNonQuery();

			SetVersion(conn, 1);
		}

		// ----------------------------
		// MIGRATION 2
		// ----------------------------

		private void Migration2_AddAnalyticsColumns(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();

			if (!ColumnExists(conn, "StockItems", "SupplierLeadTimeDays"))
			{
				cmd.CommandText =
				@"ALTER TABLE StockItems
          ADD COLUMN SupplierLeadTimeDays INTEGER DEFAULT 7;";
				cmd.ExecuteNonQuery();
			}

			if (!ColumnExists(conn, "StockItems", "SafetyStockDays"))
			{
				cmd.CommandText =
				@"ALTER TABLE StockItems
          ADD COLUMN SafetyStockDays INTEGER DEFAULT 3;";
				cmd.ExecuteNonQuery();
			}

			SetVersion(conn, 2);
		}

		// ----------------------------
		// MIGRATION 3
		// ----------------------------

		private void Migration3_AddBarcodeFields(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();

			if (!ColumnExists(conn, "StockItems", "SKU"))
			{
				cmd.CommandText =
				@"ALTER TABLE StockItems
          ADD COLUMN SKU TEXT;";
				cmd.ExecuteNonQuery();
			}

			if (!ColumnExists(conn, "StockItems", "Barcode"))
			{
				cmd.CommandText =
				@"ALTER TABLE StockItems
          ADD COLUMN Barcode TEXT;";
				cmd.ExecuteNonQuery();
			}

			SetVersion(conn, 3);
		}

		// ----------------------------
		// MIGRATION 4
		// ----------------------------

		private void Migration4_AddLocations(SqliteConnection conn)
		{
			using var cmd = conn.CreateCommand();

			cmd.CommandText =
			@"CREATE TABLE IF NOT EXISTS StockLocations(
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT
            );"
;
			cmd.CommandText =
			@"CREATE TABLE IF NOT EXISTS StockBalances(
                Id TEXT PRIMARY KEY,
                StockItemId TEXT,
                LocationId TEXT,
                Quantity INTEGER,
                FOREIGN KEY(StockItemId) REFERENCES StockItems(Id),
                FOREIGN KEY(LocationId) REFERENCES StockLocations(Id)
            );";

			cmd.ExecuteNonQuery();

			SetVersion(conn, 4);
		}
	}
}
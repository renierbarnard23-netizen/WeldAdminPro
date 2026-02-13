using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockRepository
	{
		private readonly string _connectionString;

		public StockRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		// =========================================================
		// SCHEMA
		// =========================================================
		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "PRAGMA foreign_keys = ON;";
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StockItems (
    Id TEXT PRIMARY KEY,
    ItemCode TEXT NOT NULL,
    Description TEXT,
    Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
    Unit TEXT,
    MinLevel REAL NULL,
    MaxLevel REAL NULL,
    Category TEXT NOT NULL DEFAULT 'Uncategorised',
    AverageUnitCost REAL NOT NULL DEFAULT 0
);";
			cmd.ExecuteNonQuery();

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

			EnsureBalanceAfterColumn(connection);
			EnsureProjectIdColumn(connection);
		}

		// =========================================================
		// MIGRATIONS
		// =========================================================
		private void EnsureBalanceAfterColumn(SqliteConnection connection)
		{
			if (!ColumnExists(connection, "StockTransactions", "BalanceAfter"))
			{
				using var cmd = connection.CreateCommand();
				cmd.CommandText =
					"ALTER TABLE StockTransactions ADD COLUMN BalanceAfter INTEGER;";
				cmd.ExecuteNonQuery();

				BackfillBalances(connection);
			}
		}

		private void EnsureProjectIdColumn(SqliteConnection connection)
		{
			if (!ColumnExists(connection, "StockTransactions", "ProjectId"))
			{
				using var cmd = connection.CreateCommand();
				cmd.CommandText =
					"ALTER TABLE StockTransactions ADD COLUMN ProjectId TEXT NULL;";
				cmd.ExecuteNonQuery();
			}
		}

		private bool ColumnExists(SqliteConnection connection, string table, string column)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = $"PRAGMA table_info({table});";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				if (reader.GetString(1)
					.Equals(column, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		private void BackfillBalances(SqliteConnection connection)
		{
			var balances = new Dictionary<Guid, int>();
			var updates = new List<(Guid txId, int balanceAfter)>();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, StockItemId, Quantity, Type
FROM StockTransactions
ORDER BY TransactionDate ASC;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var txId = Guid.Parse(reader.GetString(0));
				var stockId = Guid.Parse(reader.GetString(1));
				var qty = reader.GetInt32(2);
				var type = reader.GetString(3);

				if (!balances.ContainsKey(stockId))
					balances[stockId] = 0;

				balances[stockId] += type == "IN" ? qty : -qty;

				updates.Add((txId, balances[stockId]));
			}

			reader.Close();

			foreach (var u in updates)
			{
				using var updateCmd = connection.CreateCommand();
				updateCmd.CommandText =
					"UPDATE StockTransactions SET BalanceAfter=$b WHERE Id=$id;";
				updateCmd.Parameters.AddWithValue("$b", u.balanceAfter);
				updateCmd.Parameters.AddWithValue("$id", u.txId.ToString());
				updateCmd.ExecuteNonQuery();
			}
		}

		// =========================================================
		// STOCK ITEMS
		// =========================================================

		public List<StockItem> GetAll()
		{
			var list = new List<StockItem>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, ItemCode, Description, Quantity, Unit,
       MinLevel, MaxLevel, Category, AverageUnitCost
FROM StockItems
ORDER BY ItemCode;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new StockItem
				{
					Id = Guid.Parse(reader.GetString(0)),
					ItemCode = reader.GetString(1),
					Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
					Quantity = reader.GetInt32(3),
					Unit = reader.IsDBNull(4) ? "" : reader.GetString(4),
					MinLevel = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
					MaxLevel = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
					Category = reader.IsDBNull(7) ? "Uncategorised" : reader.GetString(7),
					AverageUnitCost = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8)
				});
			}

			return list;
		}

		public int GetAvailableQuantity(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Quantity FROM StockItems WHERE Id=$id;";
			cmd.Parameters.AddWithValue("$id", id.ToString());

			return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
		}

		public void Add(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
INSERT INTO StockItems
(Id, ItemCode, Description, Quantity, Unit, MinLevel, MaxLevel, Category, AverageUnitCost)
VALUES
($id, $code, $desc, $qty, $unit, $min, $max, $cat, $avg);";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$code", item.ItemCode.Trim());
			cmd.Parameters.AddWithValue("$desc", item.Description ?? "");
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit ?? "");
			cmd.Parameters.AddWithValue("$min", (object?)item.MinLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$max", (object?)item.MaxLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");
			cmd.Parameters.AddWithValue("$avg", item.AverageUnitCost);

			cmd.ExecuteNonQuery();
		}

		public void Update(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
UPDATE StockItems SET
    Description = $desc,
    Quantity = $qty,
    Unit = $unit,
    MinLevel = $min,
    MaxLevel = $max,
    Category = $cat,
    AverageUnitCost = $avg
WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$desc", item.Description ?? "");
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit ?? "");
			cmd.Parameters.AddWithValue("$min", (object?)item.MinLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$max", (object?)item.MaxLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");
			cmd.Parameters.AddWithValue("$avg", item.AverageUnitCost);

			cmd.ExecuteNonQuery();
		}

		public string GetNextItemCodeSuggestion()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT ItemCode FROM StockItems ORDER BY ItemCode DESC LIMIT 1;";

			var result = cmd.ExecuteScalar()?.ToString();

			if (string.IsNullOrWhiteSpace(result))
				return "ITEM-001";

			var parts = result.Split('-', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length < 2 || !int.TryParse(parts[^1], out int number))
				return result + "-1";

			return $"{string.Join('-', parts[..^1])}-{number + 1:000}";
		}

		// =========================================================
		// TRANSACTIONS
		// =========================================================

		public void AddTransaction(StockTransaction tx)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var dbTx = connection.BeginTransaction();
			try
			{
				int currentQty;

				using (var getCmd = connection.CreateCommand())
				{
					getCmd.Transaction = dbTx;
					getCmd.CommandText =
						"SELECT Quantity FROM StockItems WHERE Id=$id;";
					getCmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());
					currentQty = Convert.ToInt32(getCmd.ExecuteScalar());
				}

				var adjustment = tx.Type == "IN" ? tx.Quantity : -tx.Quantity;
				var newBalance = currentQty + adjustment;

				using (var insertCmd = connection.CreateCommand())
				{
					insertCmd.Transaction = dbTx;

					insertCmd.CommandText = @"
INSERT INTO StockTransactions
(Id, StockItemId, ProjectId, TransactionDate,
 Quantity, Type, UnitCost, Reference, BalanceAfter)
VALUES ($id, $stockId, $projId, $date,
        $qty, $type, $cost, $ref, $bal);";

					insertCmd.Parameters.AddWithValue("$id", tx.Id.ToString());
					insertCmd.Parameters.AddWithValue("$stockId", tx.StockItemId.ToString());
					insertCmd.Parameters.AddWithValue("$projId",
						tx.ProjectId?.ToString() ?? (object)DBNull.Value);
					insertCmd.Parameters.AddWithValue("$date", tx.TransactionDate.ToString("o"));
					insertCmd.Parameters.AddWithValue("$qty", tx.Quantity);
					insertCmd.Parameters.AddWithValue("$type", tx.Type);
					insertCmd.Parameters.AddWithValue("$cost", tx.UnitCost);
					insertCmd.Parameters.AddWithValue("$ref", tx.Reference ?? "");
					insertCmd.Parameters.AddWithValue("$bal", newBalance);

					insertCmd.ExecuteNonQuery();
				}

				using (var updateCmd = connection.CreateCommand())
				{
					updateCmd.Transaction = dbTx;
					updateCmd.CommandText =
						"UPDATE StockItems SET Quantity=$q WHERE Id=$id;";
					updateCmd.Parameters.AddWithValue("$q", newBalance);
					updateCmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());
					updateCmd.ExecuteNonQuery();
				}

				dbTx.Commit();
			}
			catch
			{
				dbTx.Rollback();
				throw;
			}
		}

		public List<StockTransaction> GetAllTransactions()
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, StockItemId, ProjectId,
       TransactionDate, Quantity, Type,
       UnitCost, Reference, BalanceAfter
FROM StockTransactions
ORDER BY TransactionDate DESC;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new StockTransaction
				{
					Id = Guid.Parse(reader.GetString(0)),
					StockItemId = Guid.Parse(reader.GetString(1)),
					ProjectId = reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
					TransactionDate = DateTime.Parse(reader.GetString(3)),
					Quantity = reader.GetInt32(4),
					Type = reader.GetString(5),
					UnitCost = reader.GetDecimal(6),
					Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
					BalanceAfter = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
				});
			}

			return list;
		}
	}
}

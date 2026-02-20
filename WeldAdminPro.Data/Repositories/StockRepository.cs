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

		public List<StockTransaction> GetProjectTransactions(Guid projectId)
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT 
    t.Id,
    t.StockItemId,
    t.ProjectId,
    t.TransactionDate,
    t.Quantity,
    t.Type,
    t.UnitCost,
    t.Reference,
    t.BalanceAfter,
    s.ItemCode,
    s.Description
FROM StockTransactions t
LEFT JOIN StockItems s ON s.Id = t.StockItemId
WHERE LOWER(t.ProjectId) = LOWER($projectId)
ORDER BY t.TransactionDate ASC, t.Id ASC;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				DateTime parsedDate;
				DateTime.TryParse(reader.GetString(3), out parsedDate);

				list.Add(new StockTransaction
				{
					Id = Guid.Parse(reader.GetString(0)),
					StockItemId = Guid.Parse(reader.GetString(1)),
					ProjectId = reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
					TransactionDate = parsedDate,
					Quantity = reader.GetInt32(4),
					Type = reader.GetString(5),
					UnitCost = reader.GetDecimal(6),
					Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
					BalanceAfter = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
					ItemCode = reader.IsDBNull(9) ? "" : reader.GetString(9),
					ItemDescription = reader.IsDBNull(10) ? "" : reader.GetString(10)
				});
			}

			return list;

		}
			public StockItem? GetById(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, ItemCode, Description, Quantity, Unit,
       MinLevel, MaxLevel, Category, AverageUnitCost
FROM StockItems
WHERE LOWER(Id) = LOWER($id);";

			cmd.Parameters.AddWithValue("$id", id.ToString());

			using var reader = cmd.ExecuteReader();

			if (!reader.Read())
				return null;

			return new StockItem
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
			};
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
    Quantity INTEGER NOT NULL,
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
			cmd.Parameters.AddWithValue("$code", item.ItemCode);
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
    Description=$desc,
    Quantity=$qty,
    Unit=$unit,
    MinLevel=$min,
    MaxLevel=$max,
    Category=$cat,
    AverageUnitCost=$avg
WHERE Id=$id;";

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

			return result;
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
				decimal currentAvgCost;

				// Get current stock state
				using (var getCmd = connection.CreateCommand())
				{
					getCmd.Transaction = dbTx;
					getCmd.CommandText =
						"SELECT Quantity, AverageUnitCost FROM StockItems WHERE Id=$id;";
					getCmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

					using var reader = getCmd.ExecuteReader();

					if (!reader.Read())
						throw new Exception("Stock item not found.");

					currentQty = reader.GetInt32(0);
					currentAvgCost = reader.GetDecimal(1);
				}

				// 🔒 STRICT MODE — Prevent negative stock
				if (tx.Type == "OUT" && tx.Quantity > currentQty)
					throw new InvalidOperationException(
						"Insufficient stock available for this OUT transaction.");

				int adjustment = tx.Type == "IN"
					? tx.Quantity
					: -tx.Quantity;

				int newQty = currentQty + adjustment;

				decimal newAvgCost = currentAvgCost;

				// 📈 Weighted Average Cost update (IN only, cost > 0)
				if (tx.Type == "IN" && tx.UnitCost > 0 && tx.Quantity > 0)
				{
					decimal totalExistingValue = currentQty * currentAvgCost;
					decimal totalIncomingValue = tx.Quantity * tx.UnitCost;

					newAvgCost = (totalExistingValue + totalIncomingValue)
								 / (currentQty + tx.Quantity);
				}

				// Insert transaction
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
					insertCmd.Parameters.AddWithValue("$bal", newQty);

					insertCmd.ExecuteNonQuery();
				}

				// Update stock item quantity + average cost
				using (var updateCmd = connection.CreateCommand())
				{
					updateCmd.Transaction = dbTx;
					updateCmd.CommandText =
						"UPDATE StockItems SET Quantity=$qty, AverageUnitCost=$avg WHERE Id=$id;";

					updateCmd.Parameters.AddWithValue("$qty", newQty);
					updateCmd.Parameters.AddWithValue("$avg", newAvgCost);
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


		// =========================================================
		// REPAIR
		// =========================================================

		public void RepairOpeningBalances()
		{
			// same safe surgical logic retained
			RecalculateAllBalances();
		}

		public void RecalculateAllBalances()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var dbTx = connection.BeginTransaction();

			var transactions = new List<(Guid Id, Guid StockId, int Qty, string Type)>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.Transaction = dbTx;
				cmd.CommandText = @"
SELECT Id, StockItemId, Quantity, Type
FROM StockTransactions
ORDER BY TransactionDate ASC, Id ASC;";

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					transactions.Add((
						Guid.Parse(reader.GetString(0)),
						Guid.Parse(reader.GetString(1)),
						reader.GetInt32(2),
						reader.GetString(3)
					));
				}
			}

			var balances = new Dictionary<Guid, int>();

			foreach (var tx in transactions)
			{
				if (!balances.ContainsKey(tx.StockId))
					balances[tx.StockId] = 0;

				balances[tx.StockId] += tx.Type == "IN"
					? tx.Qty
					: -tx.Qty;

				using var updateCmd = connection.CreateCommand();
				updateCmd.Transaction = dbTx;
				updateCmd.CommandText =
					"UPDATE StockTransactions SET BalanceAfter=$bal WHERE Id=$id;";
				updateCmd.Parameters.AddWithValue("$bal", balances[tx.StockId]);
				updateCmd.Parameters.AddWithValue("$id", tx.Id.ToString());
				updateCmd.ExecuteNonQuery();
			}

			foreach (var item in balances)
			{
				using var updateStockCmd = connection.CreateCommand();
				updateStockCmd.Transaction = dbTx;
				updateStockCmd.CommandText =
					"UPDATE StockItems SET Quantity=$qty WHERE Id=$id;";
				updateStockCmd.Parameters.AddWithValue("$qty", item.Value);
				updateStockCmd.Parameters.AddWithValue("$id", item.Key.ToString());
				updateStockCmd.ExecuteNonQuery();
			}

			dbTx.Commit();
		}

		// =========================================================
		// HISTORY
		// =========================================================

		public List<StockTransaction> GetAllTransactions()
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT 
    t.Id,
    t.StockItemId,
    t.ProjectId,
    t.TransactionDate,
    t.Quantity,
    t.Type,
    t.UnitCost,
    t.Reference,
    t.BalanceAfter,
    p.ProjectName,
    s.ItemCode,
    s.Description
FROM StockTransactions t
LEFT JOIN Projects p ON LOWER(p.Id) = LOWER(t.ProjectId)
LEFT JOIN StockItems s ON s.Id = t.StockItemId
ORDER BY t.TransactionDate ASC, t.Id ASC;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				DateTime parsedDate;
				DateTime.TryParse(reader.GetString(3), out parsedDate);

				list.Add(new StockTransaction
				{
					Id = Guid.Parse(reader.GetString(0)),
					StockItemId = Guid.Parse(reader.GetString(1)),
					ProjectId = reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
					TransactionDate = parsedDate,
					Quantity = reader.GetInt32(4),
					Type = reader.GetString(5),
					UnitCost = reader.GetDecimal(6),
					Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
					BalanceAfter = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
					ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
					ItemCode = reader.IsDBNull(10) ? "" : reader.GetString(10),
					ItemDescription = reader.IsDBNull(11) ? "" : reader.GetString(11)
				});
			}

			return list;
		}
	}
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockRepository
	{
		private readonly string _connectionString;

		public StockRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
		}

		// =========================================================
		// STOCK ITEM LOOKUP
		// =========================================================

		public StockItem? GetById(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, ItemCode, Description, Quantity, Unit,
       MinLevel, MaxLevel, Category, AverageUnitCost
FROM StockItems
WHERE Id = $id;";

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
($id,$code,$desc,$qty,$unit,$min,$max,$cat,$avg);";

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
		// STOCK TRANSACTIONS
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

				using (var cmd = connection.CreateCommand())
				{
					cmd.Transaction = dbTx;

					cmd.CommandText =
						"SELECT Quantity, AverageUnitCost FROM StockItems WHERE Id=$id;";

					cmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

					using var reader = cmd.ExecuteReader();

					if (!reader.Read())
						throw new Exception("Stock item not found.");

					currentQty = reader.GetInt32(0);
					currentAvgCost = reader.GetDecimal(1);
				}

				if (tx.Type != "IN" && tx.Type != "OUT" && tx.Type != "RET")
					throw new InvalidOperationException("Invalid transaction type.");

				if (tx.Quantity <= 0)
					throw new InvalidOperationException("Quantity must be greater than zero.");

				if (tx.Type == "OUT" && tx.Quantity > currentQty)
					throw new InvalidOperationException(
						$"Insufficient stock. Available: {currentQty}, Requested: {tx.Quantity}");

				int adjustment =
					tx.Type == "IN" || tx.Type == "RET"
					? tx.Quantity
					: -tx.Quantity;

				int newQty = currentQty + adjustment;

				decimal newAvgCost = currentAvgCost;

				if (tx.Type == "IN" && tx.UnitCost > 0)
				{
					decimal totalExistingValue = currentQty * currentAvgCost;
					decimal totalIncomingValue = tx.Quantity * tx.UnitCost;

					newAvgCost =
						(totalExistingValue + totalIncomingValue)
						/ (currentQty + tx.Quantity);
				}

				using (var insert = connection.CreateCommand())
				{
					insert.Transaction = dbTx;

					insert.CommandText = @"
INSERT INTO StockTransactions
(Id,StockItemId,ProjectId,TransactionDate,
 Quantity,Type,UnitCost,Reference,BalanceAfter)
VALUES
($id,$stock,$proj,$date,$qty,$type,$cost,$ref,$bal);";

					insert.Parameters.AddWithValue("$id", tx.Id.ToString());
					insert.Parameters.AddWithValue("$stock", tx.StockItemId.ToString());
					insert.Parameters.AddWithValue("$proj",
						tx.ProjectId?.ToString() ?? (object)DBNull.Value);
					insert.Parameters.AddWithValue("$date",
						tx.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"));
					insert.Parameters.AddWithValue("$qty", tx.Quantity);
					insert.Parameters.AddWithValue("$type", tx.Type);
					insert.Parameters.AddWithValue("$cost", tx.UnitCost);
					insert.Parameters.AddWithValue("$ref", tx.Reference ?? "");
					insert.Parameters.AddWithValue("$bal", newQty);

					insert.ExecuteNonQuery();
				}

				using (var update = connection.CreateCommand())
				{
					update.Transaction = dbTx;

					update.CommandText =
						"UPDATE StockItems SET Quantity=$qty, AverageUnitCost=$avg WHERE Id=$id;";

					update.Parameters.AddWithValue("$qty", newQty);
					update.Parameters.AddWithValue("$avg", newAvgCost);
					update.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

					update.ExecuteNonQuery();
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
		// PROJECT TRANSACTIONS
		// =========================================================

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
WHERE t.ProjectId = $projectId
ORDER BY t.TransactionDate ASC;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				DateTime.TryParse(reader.GetString(3), out DateTime parsedDate);

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
    s.ItemCode,
    s.Description,
    p.ProjectName
FROM StockTransactions t
LEFT JOIN StockItems s ON s.Id = t.StockItemId
LEFT JOIN Projects p ON p.Id = t.ProjectId
ORDER BY t.TransactionDate ASC, t.Id ASC;";

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				DateTime.TryParse(reader.GetString(3), out DateTime parsedDate);

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
					ItemDescription = reader.IsDBNull(10) ? "" : reader.GetString(10),
					ProjectName = reader.IsDBNull(11) ? null : reader.GetString(11)
				});
			}

			return list;
		}
		public void UpdateTransactionBalance(Guid transactionId, int newBalance)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE StockTransactions
SET BalanceAfter = $balance
WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$balance", newBalance);
			cmd.Parameters.AddWithValue("$id", transactionId.ToString());

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
		public List<StockTransaction> GetIssuedMaterials(Guid projectId)
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
    s.ItemCode,
    s.Description
FROM StockTransactions t
LEFT JOIN StockItems s ON s.Id = t.StockItemId
WHERE t.ProjectId = $projectId
AND t.Type = 'OUT'
ORDER BY t.TransactionDate DESC;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new StockTransaction
				{
					Id = Guid.Parse(reader.GetString(0)),
					StockItemId = Guid.Parse(reader.GetString(1)),
					ProjectId = Guid.Parse(reader.GetString(2)),
					TransactionDate = DateTime.Parse(reader.GetString(3)),
					Quantity = reader.GetInt32(4),
					Type = reader.GetString(5),
					UnitCost = reader.GetDecimal(6),
					Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
					ItemCode = reader.IsDBNull(8) ? "" : reader.GetString(8),
					ItemDescription = reader.IsDBNull(9) ? "" : reader.GetString(9)
				});
			}

			return list;
		}
		public IEnumerable<StockTransaction> GetReturnableItems(Guid projectId)
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
    s.ItemCode,
    s.Description
FROM StockTransactions t
LEFT JOIN StockItems s ON s.Id = t.StockItemId
WHERE t.ProjectId = $projectId
AND t.Type = 'OUT'
ORDER BY t.TransactionDate DESC;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new StockTransaction
				{
					Id = Guid.Parse(reader.GetString(0)),
					StockItemId = Guid.Parse(reader.GetString(1)),
					ProjectId = Guid.Parse(reader.GetString(2)),
					TransactionDate = DateTime.Parse(reader.GetString(3)),
					Quantity = reader.GetInt32(4),
					Type = reader.GetString(5),
					UnitCost = reader.GetDecimal(6),
					Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
					ItemCode = reader.IsDBNull(8) ? "" : reader.GetString(8),
					ItemDescription = reader.IsDBNull(9) ? "" : reader.GetString(9)
				});
			}

			return list;
		}

		// =========================================================
		// LEDGER / AUDIT SUPPORT
		// =========================================================

		public int GetCurrentStock(Guid stockItemId)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
SELECT Quantity
FROM StockItems
WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

			return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
		}

		public List<StockTransaction> GetTransactionsByDateRange(DateTime? start, DateTime? end)
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
WHERE ($start IS NULL OR t.TransactionDate >= $start)
AND ($end IS NULL OR t.TransactionDate <= $end)
ORDER BY t.TransactionDate ASC;";

			cmd.Parameters.AddWithValue("$start",
				start?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);

			cmd.Parameters.AddWithValue("$end",
				end?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				DateTime.TryParse(reader.GetString(3), out DateTime parsedDate);

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

		public void RecalculateAllBalances()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var transactions = GetAllTransactions();

			var grouped = transactions
				.GroupBy(t => t.StockItemId);

			foreach (var group in grouped)
			{
				int running = 0;

				foreach (var tx in group.OrderBy(t => t.TransactionDate))
				{
					if (tx.Type == "IN" || tx.Type == "RET")
						running += tx.Quantity;
					else if (tx.Type == "OUT")
						running -= tx.Quantity;

					UpdateTransactionBalance(tx.Id, running);
				}
			}
		}

		public List<StockTransaction> GetAuditLog()
		{
			return GetAllTransactions()
				.OrderByDescending(t => t.TransactionDate)
				.ToList();
		}
	}
}
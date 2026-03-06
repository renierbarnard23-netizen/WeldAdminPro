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

		private readonly StockTransactionRepository _transactionRepo;
		public StockRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			_transactionRepo = new StockTransactionRepository();
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

		// =========================================================
		// LEDGER SUPPORT
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
		// =========================================================
		// TRANSACTION BRIDGE (Temporary During Refactor)
		// =========================================================

		public List<StockTransaction> GetAllTransactions()
		{
			return _transactionRepo.GetAllTransactions();
		}

		public void AddTransaction(StockTransaction tx)
		{
			_transactionRepo.AddTransaction(tx);
		}

		public List<StockTransaction> GetAuditLog()
		{
			return _transactionRepo
				.GetAllTransactions()
				.OrderByDescending(t => t.TransactionDate)
				.ToList();
		}
		public List<StockTransaction> GetProjectTransactions(Guid projectId)
		{
			return _transactionRepo.GetProjectTransactions(projectId);
		}

		public List<StockTransaction> GetIssuedMaterials(Guid projectId)
		{
			return _transactionRepo.GetIssuedMaterials(projectId);
		}

		public IEnumerable<StockTransaction> GetReturnableItems(Guid projectId)
		{
			return _transactionRepo.GetReturnableItems(projectId);
		}

		public List<StockTransaction> GetTransactionsByDateRange(DateTime? start, DateTime? end)
		{
			return _transactionRepo.GetTransactionsByDateRange(start, end);
		}

		public void UpdateTransactionBalance(Guid transactionId, int newBalance)
		{
			_transactionRepo.UpdateTransactionBalance(transactionId, newBalance);
		}

		public void RecalculateAllBalances()
		{
			var transactions = _transactionRepo.GetAllTransactions();

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

					_transactionRepo.UpdateTransactionBalance(tx.Id, running);
				}
			}
		}
	}
}
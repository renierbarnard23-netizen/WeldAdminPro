using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM StockItems;";
            Console.WriteLine("StockItems Count = " + countCmd.ExecuteScalar());

            Console.WriteLine("StockRepository DB = " + connection.DataSource);

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

            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }
            using var connection = new SqliteConnection(_connectionString);
			connection.Open();

            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM StockItems;";
            Console.WriteLine("StockItems Count = " + countCmd.ExecuteScalar());

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

			// Extract number part
			var numberPart = result.Replace("ITEM-", "");

			if (!int.TryParse(numberPart, out int number))
				return "ITEM-001";

			number++;

			return $"ITEM-{number:000}";
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
            // Load stock item
            var item = GetById(tx.StockItemId);

            if (item == null)
                throw new Exception("Stock item not found.");

            // Apply quantity movement
            if (tx.Type == "IN")
            {
                decimal oldQty = (decimal)item.Quantity;
                decimal receivedQty = (decimal)tx.Quantity;
                decimal newQty = oldQty + receivedQty;

                if (newQty > 0)
                {
                    item.AverageUnitCost =
                        ((oldQty * item.AverageUnitCost) +
                         (receivedQty * tx.UnitCost))
                        / newQty;
                }

                item.Quantity = (double)newQty;
            }
            else if (tx.Type == "OUT")
            {
                item.Quantity -= (int)tx.Quantity;

                if (item.Quantity < 0)
                    throw new Exception("Insufficient stock.");
            }

            // Running balance
            tx.BalanceAfter = (int)Math.Round(item.Quantity);

            // Update stock master
            Update(item);

            // Save transaction history
            _transactionRepo.AddTransaction(tx);

            // Keep ledger consistent
            RecalculateAllBalances();

            // Auto production reschedule
            // var rescheduler = new ProductionReschedulerService();
            // rescheduler.RecalculateProduction();
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

        public IEnumerable<ReturnableItemDto> GetReturnableItems(Guid projectId)
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
		public bool TryDeductStock(SqliteConnection connection, SqliteTransaction transaction, string itemCode, double quantity)
		{
			// Get current stock
			using var getCmd = connection.CreateCommand();
			getCmd.Transaction = transaction;
			getCmd.CommandText = "SELECT Quantity FROM StockItems WHERE ItemCode = $code";
			getCmd.Parameters.AddWithValue("$code", itemCode);

			var result = getCmd.ExecuteScalar();

			if (result == null)
				return false;

			double currentQty = Convert.ToDouble(result);

			// Prevent negative stock
			if (currentQty < quantity)
				return false;

			// Deduct stock
			using var updateCmd = connection.CreateCommand();
			updateCmd.Transaction = transaction;
			updateCmd.CommandText = "UPDATE StockItems SET Quantity = Quantity - $qty WHERE ItemCode = $code";
			updateCmd.Parameters.AddWithValue("$qty", quantity);
			updateCmd.Parameters.AddWithValue("$code", itemCode);

			updateCmd.ExecuteNonQuery();

			return true;
		}
		public StockItem? GetByItemCode(string itemCode)
		{
			if (string.IsNullOrWhiteSpace(itemCode))
				return null;

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM StockItems WHERE ItemCode = @code";
			cmd.Parameters.AddWithValue("@code", itemCode);

			using var reader = cmd.ExecuteReader();

			if (reader.Read())
			{
				return new StockItem
				{
					Id = Guid.Parse(reader["Id"].ToString()!),
					ItemCode = reader["ItemCode"].ToString() ?? "",
					Description = reader["Description"].ToString() ?? "",
					Quantity = Convert.ToDouble(reader["Quantity"]),
					AverageUnitCost = Convert.ToDecimal(reader["AverageUnitCost"])
				};
			}

			return null;
		}
		public List<WorkOrderMaterialTrace> GetMaterialTraceForWorkOrder(string workOrderNumber)
		{
			var list = new List<WorkOrderMaterialTrace>();

			try
			{
				using var connection = new SqliteConnection(_connectionString);
				connection.Open();

				Debug.WriteLine($"🔥 SEARCHING TRACE FOR: [{workOrderNumber}]");

				using var cmd = connection.CreateCommand();

				cmd.CommandText = @"
		SELECT 
			si.ItemCode,
			si.Description,
			st.Quantity,
			st.UnitCost,
			st.Reference
		FROM StockTransactions st
		JOIN StockItems si ON st.StockItemId = si.Id
		WHERE st.Reference LIKE @ref";

				cmd.Parameters.AddWithValue("@ref", $"%{workOrderNumber}%");

				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					list.Add(new WorkOrderMaterialTrace
					{
						ItemCode = reader["ItemCode"]?.ToString() ?? "",
						Description = reader["Description"]?.ToString() ?? "",
						Quantity = Convert.ToInt32(reader["Quantity"]),
						UnitCost = Convert.ToDecimal(reader["UnitCost"])
					});
				}

				Debug.WriteLine($"📊 TRACE COUNT: {list.Count}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ TRACE ERROR:");
				Debug.WriteLine(ex.Message);
			}

			return list;
		}

	}
}
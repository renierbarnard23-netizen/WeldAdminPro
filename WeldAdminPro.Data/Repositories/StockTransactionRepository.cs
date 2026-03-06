using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockTransactionRepository
	{
		private readonly string _connectionString;

		public StockTransactionRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
		}

		// =========================================================
		// ADD TRANSACTION
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
		// TRANSACTION QUERIES
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
	}
}
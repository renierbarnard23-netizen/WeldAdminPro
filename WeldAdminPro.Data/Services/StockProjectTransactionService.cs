using Microsoft.Data.Sqlite;
using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
	public class StockProjectTransactionService
	{
		private readonly string _connectionString;

		public StockProjectTransactionService()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
		}

		// =========================================================
		// ISSUE STOCK (ATOMIC + HISTORY SAFE)
		// =========================================================
		public void IssueStock(
			Project project,
			StockItem stockItem,
			decimal quantity,
			string issuedBy)
		{
			if (quantity <= 0)
				throw new InvalidOperationException("Quantity must be greater than zero.");

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			using var tx = connection.BeginTransaction();

			try
			{
				// -------------------------------------------------
				// 1. CHECK CURRENT STOCK
				// -------------------------------------------------
				using var checkCmd = connection.CreateCommand();
				checkCmd.Transaction = tx;
				checkCmd.CommandText =
					"SELECT Quantity FROM StockItems WHERE Id = $id;";
				checkCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());

				int currentQty = Convert.ToInt32(checkCmd.ExecuteScalar());

				if (currentQty < quantity)
					throw new InvalidOperationException("Insufficient stock available.");

				int newBalance = currentQty - (int)quantity;

				// -------------------------------------------------
				// 2. INSERT INTO STOCK TRANSACTIONS (HISTORY)
				// -------------------------------------------------
				using var insertTxCmd = connection.CreateCommand();
				insertTxCmd.Transaction = tx;
				insertTxCmd.CommandText = @"
INSERT INTO StockTransactions
(Id, StockItemId, ProjectId, TransactionDate,
 Quantity, Type, UnitCost, Reference, BalanceAfter)
VALUES
($id, $stockId, $projectId, $date,
 $qty, 'OUT', $cost, $ref, $balance);";

				insertTxCmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
				insertTxCmd.Parameters.AddWithValue("$stockId", stockItem.Id.ToString());
				insertTxCmd.Parameters.AddWithValue("$projectId", project.Id.ToString());
				insertTxCmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));
				insertTxCmd.Parameters.AddWithValue("$qty", quantity);
				insertTxCmd.Parameters.AddWithValue("$cost", stockItem.AverageUnitCost);
				insertTxCmd.Parameters.AddWithValue("$ref", project.JobNumber.ToString());
				insertTxCmd.Parameters.AddWithValue("$balance", newBalance);

				insertTxCmd.ExecuteNonQuery();

				// -------------------------------------------------
				// 3. INSERT PROJECT USAGE
				// -------------------------------------------------
				using var usageCmd = connection.CreateCommand();
				usageCmd.Transaction = tx;
				usageCmd.CommandText = @"
INSERT INTO ProjectStockUsages
(Id, ProjectId, StockItemId, Quantity, UnitCostAtIssue, IssuedOn, IssuedBy, Notes)
VALUES
($id, $projectId, $stockItemId, $qty, $cost, $issuedOn, $issuedBy, $notes);";

				usageCmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
				usageCmd.Parameters.AddWithValue("$projectId", project.Id.ToString());
				usageCmd.Parameters.AddWithValue("$stockItemId", stockItem.Id.ToString());
				usageCmd.Parameters.AddWithValue("$qty", quantity);
				usageCmd.Parameters.AddWithValue("$cost", stockItem.AverageUnitCost);
				usageCmd.Parameters.AddWithValue("$issuedOn", DateTime.UtcNow.ToString("o"));
				usageCmd.Parameters.AddWithValue("$issuedBy", issuedBy ?? "");
				usageCmd.Parameters.AddWithValue("$notes", stockItem.Description ?? "");

				usageCmd.ExecuteNonQuery();

				// -------------------------------------------------
				// 4. UPDATE STOCK QUANTITY
				// -------------------------------------------------
				using var updateStockCmd = connection.CreateCommand();
				updateStockCmd.Transaction = tx;
				updateStockCmd.CommandText =
					"UPDATE StockItems SET Quantity = $qty WHERE Id = $id;";
				updateStockCmd.Parameters.AddWithValue("$qty", newBalance);
				updateStockCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());
				updateStockCmd.ExecuteNonQuery();

				// -------------------------------------------------
				// 5. UPDATE PROJECT COST
				// -------------------------------------------------
				decimal issueCost = quantity * stockItem.AverageUnitCost;
				project.ActualCost += issueCost;
				project.LastModifiedOn = DateTime.UtcNow;

				using var updateProjectCmd = connection.CreateCommand();
				updateProjectCmd.Transaction = tx;
				updateProjectCmd.CommandText = @"
UPDATE Projects
SET ActualCost = $cost,
	LastModifiedOn = $modified
WHERE Id = $id;";

				updateProjectCmd.Parameters.AddWithValue("$cost", project.ActualCost);
				updateProjectCmd.Parameters.AddWithValue("$modified", project.LastModifiedOn?.ToString("o"));
				updateProjectCmd.Parameters.AddWithValue("$id", project.Id.ToString());
				updateProjectCmd.ExecuteNonQuery();

				tx.Commit();
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}

		// =========================================================
		// RETURN STOCK (ATOMIC + HISTORY SAFE)
		// =========================================================
		public void ReturnStock(
			Project project,
			StockItem stockItem,
			decimal quantity,
			decimal originalUnitCost,
			string issuedBy)
		{
			if (quantity <= 0)
				throw new InvalidOperationException("Quantity must be greater than zero.");

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			using var tx = connection.BeginTransaction();

			try
			{
				// 1. Get current stock
				using var checkCmd = connection.CreateCommand();
				checkCmd.Transaction = tx;
				checkCmd.CommandText =
					"SELECT Quantity FROM StockItems WHERE Id = $id;";
				checkCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());

				int currentQty = Convert.ToInt32(checkCmd.ExecuteScalar());
				int newBalance = currentQty + (int)quantity;

				// 2. Insert Stock Transaction (IN)
				using var insertTxCmd = connection.CreateCommand();
				insertTxCmd.Transaction = tx;
				insertTxCmd.CommandText = @"
INSERT INTO StockTransactions
(Id, StockItemId, ProjectId, TransactionDate,
 Quantity, Type, UnitCost, Reference, BalanceAfter)
VALUES
($id, $stockId, $projectId, $date,
 $qty, 'IN', $cost, $ref, $balance);";

				insertTxCmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
				insertTxCmd.Parameters.AddWithValue("$stockId", stockItem.Id.ToString());
				insertTxCmd.Parameters.AddWithValue("$projectId", project.Id.ToString());
				insertTxCmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));
				insertTxCmd.Parameters.AddWithValue("$qty", quantity);
				insertTxCmd.Parameters.AddWithValue("$cost", originalUnitCost);
				insertTxCmd.Parameters.AddWithValue("$ref", project.JobNumber.ToString());
				insertTxCmd.Parameters.AddWithValue("$balance", newBalance);

				insertTxCmd.ExecuteNonQuery();

				// 3. Insert Project Usage (negative)
				using var usageCmd = connection.CreateCommand();
				usageCmd.Transaction = tx;
				usageCmd.CommandText = @"
INSERT INTO ProjectStockUsages
(Id, ProjectId, StockItemId, Quantity, UnitCostAtIssue, IssuedOn, IssuedBy, Notes)
VALUES
($id, $projectId, $stockItemId, $qty, $cost, $issuedOn, $issuedBy, $notes);";

				usageCmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
				usageCmd.Parameters.AddWithValue("$projectId", project.Id.ToString());
				usageCmd.Parameters.AddWithValue("$stockItemId", stockItem.Id.ToString());
				usageCmd.Parameters.AddWithValue("$qty", -quantity);
				usageCmd.Parameters.AddWithValue("$cost", originalUnitCost);
				usageCmd.Parameters.AddWithValue("$issuedOn", DateTime.UtcNow.ToString("o"));
				usageCmd.Parameters.AddWithValue("$issuedBy", issuedBy ?? "");
				usageCmd.Parameters.AddWithValue("$notes", stockItem.Description ?? "");

				usageCmd.ExecuteNonQuery();

				// 4. Update Stock
				using var updateStockCmd = connection.CreateCommand();
				updateStockCmd.Transaction = tx;
				updateStockCmd.CommandText =
					"UPDATE StockItems SET Quantity = $qty WHERE Id = $id;";
				updateStockCmd.Parameters.AddWithValue("$qty", newBalance);
				updateStockCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());
				updateStockCmd.ExecuteNonQuery();

				// 5. Reverse cost
				decimal returnCost = quantity * originalUnitCost;
				project.ActualCost -= returnCost;
				if (project.ActualCost < 0)
					project.ActualCost = 0;

				project.LastModifiedOn = DateTime.UtcNow;

				using var updateProjectCmd = connection.CreateCommand();
				updateProjectCmd.Transaction = tx;
				updateProjectCmd.CommandText = @"
UPDATE Projects
SET ActualCost = $cost,
	LastModifiedOn = $modified
WHERE Id = $id;";

				updateProjectCmd.Parameters.AddWithValue("$cost", project.ActualCost);
				updateProjectCmd.Parameters.AddWithValue("$modified", project.LastModifiedOn?.ToString("o"));
				updateProjectCmd.Parameters.AddWithValue("$id", project.Id.ToString());
				updateProjectCmd.ExecuteNonQuery();

				tx.Commit();
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}
	}
}

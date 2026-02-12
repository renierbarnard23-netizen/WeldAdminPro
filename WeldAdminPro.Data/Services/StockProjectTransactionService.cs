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
		// ISSUE STOCK (ATOMIC)
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
				// Check stock
				using var checkCmd = connection.CreateCommand();
				checkCmd.Transaction = tx;
				checkCmd.CommandText =
					"SELECT Quantity FROM StockItems WHERE Id = $id;";
				checkCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());

				int currentQty = Convert.ToInt32(checkCmd.ExecuteScalar());

				if (currentQty < quantity)
					throw new InvalidOperationException("Insufficient stock available.");

				// Insert usage
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

				// Reduce stock
				using var updateStockCmd = connection.CreateCommand();
				updateStockCmd.Transaction = tx;
				updateStockCmd.CommandText =
					"UPDATE StockItems SET Quantity = Quantity - $qty WHERE Id = $id;";
				updateStockCmd.Parameters.AddWithValue("$qty", quantity);
				updateStockCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());
				updateStockCmd.ExecuteNonQuery();

				// Update project financials
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
		// RETURN STOCK (ATOMIC)
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
				// Insert return usage (negative)
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

				// Increase stock
				using var updateStockCmd = connection.CreateCommand();
				updateStockCmd.Transaction = tx;
				updateStockCmd.CommandText =
					"UPDATE StockItems SET Quantity = Quantity + $qty WHERE Id = $id;";
				updateStockCmd.Parameters.AddWithValue("$qty", quantity);
				updateStockCmd.Parameters.AddWithValue("$id", stockItem.Id.ToString());
				updateStockCmd.ExecuteNonQuery();

				// Reverse cost using ORIGINAL unit cost
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

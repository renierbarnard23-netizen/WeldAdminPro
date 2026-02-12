using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class ProjectStockUsageRepository
	{
		private readonly string _connectionString;

		public ProjectStockUsageRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = "PRAGMA foreign_keys = ON;";
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ProjectStockUsages (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    StockItemId TEXT NOT NULL,
    Quantity REAL NOT NULL,
    UnitCostAtIssue REAL NOT NULL DEFAULT 0,
    IssuedOn TEXT NOT NULL,
    IssuedBy TEXT,
    Notes TEXT,

    FOREIGN KEY (ProjectId)
        REFERENCES Projects(Id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    FOREIGN KEY (StockItemId)
        REFERENCES StockItems(Id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);";
			cmd.ExecuteNonQuery();
		}

		// =========================================================
		// HARD PROTECTION: NO OVER-RETURN
		// =========================================================
		public void Add(ProjectStockUsage usage)
		{
			if (usage.Id == Guid.Empty)
				usage.Id = Guid.NewGuid();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			using var transaction = connection.BeginTransaction();

			decimal currentBalance;

			using (var balanceCmd = connection.CreateCommand())
			{
				balanceCmd.Transaction = transaction;
				balanceCmd.CommandText = @"
SELECT COALESCE(SUM(Quantity), 0)
FROM ProjectStockUsages
WHERE ProjectId = $projectId
  AND StockItemId = $stockItemId;";

				balanceCmd.Parameters.AddWithValue("$projectId", usage.ProjectId.ToString());
				balanceCmd.Parameters.AddWithValue("$stockItemId", usage.StockItemId.ToString());

				currentBalance = Convert.ToDecimal(balanceCmd.ExecuteScalar());
			}

			if (usage.Quantity < 0 && currentBalance + usage.Quantity < 0)
			{
				throw new InvalidOperationException(
					$"Invalid stock return.\n\n" +
					$"Issued balance: {currentBalance}\n" +
					$"Attempted return: {-usage.Quantity}"
				);
			}

			using (var insertCmd = connection.CreateCommand())
			{
				insertCmd.Transaction = transaction;
				insertCmd.CommandText = @"
INSERT INTO ProjectStockUsages
(Id, ProjectId, StockItemId, Quantity, UnitCostAtIssue, IssuedOn, IssuedBy, Notes)
VALUES
($id, $projectId, $stockItemId, $qty, $cost, $issuedOn, $issuedBy, $notes);";

				insertCmd.Parameters.AddWithValue("$id", usage.Id.ToString());
				insertCmd.Parameters.AddWithValue("$projectId", usage.ProjectId.ToString());
				insertCmd.Parameters.AddWithValue("$stockItemId", usage.StockItemId.ToString());
				insertCmd.Parameters.AddWithValue("$qty", usage.Quantity);
				insertCmd.Parameters.AddWithValue("$cost", usage.UnitCostAtIssue);
				insertCmd.Parameters.AddWithValue("$issuedOn", usage.IssuedOn.ToString("o"));
				insertCmd.Parameters.AddWithValue("$issuedBy", usage.IssuedBy ?? string.Empty);
				insertCmd.Parameters.AddWithValue("$notes", usage.Notes ?? string.Empty);

				insertCmd.ExecuteNonQuery();
			}

			transaction.Commit();
		}

		public List<ProjectStockUsage> GetByProjectId(Guid projectId)
		{
			var list = new List<ProjectStockUsage>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT
    Id,
    ProjectId,
    StockItemId,
    Quantity,
    UnitCostAtIssue,
    IssuedOn,
    IssuedBy,
    Notes
FROM ProjectStockUsages
WHERE ProjectId = $projectId
ORDER BY IssuedOn DESC;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new ProjectStockUsage
				{
					Id = Guid.Parse(reader.GetString(0)),
					ProjectId = Guid.Parse(reader.GetString(1)),
					StockItemId = Guid.Parse(reader.GetString(2)),
					Quantity = reader.GetDecimal(3),
					UnitCostAtIssue = reader.GetDecimal(4),
					IssuedOn = DateTime.Parse(reader.GetString(5)),
					IssuedBy = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
					Notes = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
				});
			}

			return list;
		}
		// ================= ADD (ATOMIC OVERLOAD) =================

		public void Add(ProjectStockUsage usage,
						SqliteConnection connection,
						SqliteTransaction transaction)
		{
			decimal currentBalance;

			using (var balanceCmd = connection.CreateCommand())
			{
				balanceCmd.Transaction = transaction;
				balanceCmd.CommandText = @"
SELECT COALESCE(SUM(Quantity), 0)
FROM ProjectStockUsages
WHERE ProjectId = $projectId
  AND StockItemId = $stockItemId;";

				balanceCmd.Parameters.AddWithValue("$projectId", usage.ProjectId.ToString());
				balanceCmd.Parameters.AddWithValue("$stockItemId", usage.StockItemId.ToString());

				currentBalance = Convert.ToDecimal(balanceCmd.ExecuteScalar());
			}

			if (usage.Quantity < 0 && currentBalance + usage.Quantity < 0)
				throw new InvalidOperationException("Invalid stock return – exceeds issued balance.");

			using var insertCmd = connection.CreateCommand();
			insertCmd.Transaction = transaction;

			insertCmd.CommandText = @"
INSERT INTO ProjectStockUsages
(Id, ProjectId, StockItemId, Quantity, UnitCostAtIssue, IssuedOn, IssuedBy, Notes)
VALUES
($id, $projectId, $stockItemId, $qty, $cost, $issuedOn, $issuedBy, $notes);";

			insertCmd.Parameters.AddWithValue("$id", usage.Id.ToString());
			insertCmd.Parameters.AddWithValue("$projectId", usage.ProjectId.ToString());
			insertCmd.Parameters.AddWithValue("$stockItemId", usage.StockItemId.ToString());
			insertCmd.Parameters.AddWithValue("$qty", usage.Quantity);
			insertCmd.Parameters.AddWithValue("$cost", usage.UnitCostAtIssue);
			insertCmd.Parameters.AddWithValue("$issuedOn", usage.IssuedOn.ToString("o"));
			insertCmd.Parameters.AddWithValue("$issuedBy", usage.IssuedBy ?? string.Empty);
			insertCmd.Parameters.AddWithValue("$notes", usage.Notes ?? string.Empty);

			insertCmd.ExecuteNonQuery();
		}


		// =========================================================
		// PROJECT STOCK SUMMARY
		// =========================================================
		public List<ProjectStockSummary> GetProjectStockSummary(Guid projectId)
		{
			var list = new List<ProjectStockSummary>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT
    s.Id,
    s.ItemCode,
    s.Description,
    s.Unit,
    SUM(CASE WHEN u.Quantity > 0 THEN u.Quantity ELSE 0 END) AS IssuedQty,
    ABS(SUM(CASE WHEN u.Quantity < 0 THEN u.Quantity ELSE 0 END)) AS ReturnedQty
FROM ProjectStockUsages u
JOIN StockItems s ON s.Id = u.StockItemId
WHERE u.ProjectId = $projectId
GROUP BY s.Id, s.ItemCode, s.Description, s.Unit
ORDER BY s.ItemCode;";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new ProjectStockSummary
				{
					StockItemId = Guid.Parse(reader.GetString(0)),
					ItemCode = reader.GetString(1),
					Description = reader.GetString(2),
					Unit = reader.IsDBNull(3) ? "" : reader.GetString(3),
					IssuedQuantity = reader.GetDecimal(4),
					ReturnedQuantity = reader.GetDecimal(5)
				});
			}

			return list;
		}
	}
}

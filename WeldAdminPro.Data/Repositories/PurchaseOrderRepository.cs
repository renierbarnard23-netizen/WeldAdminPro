using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class PurchaseOrderRepository
	{
		private readonly string _connectionString;

		public PurchaseOrderRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			
		}

		

		// =========================================================
		// SAFE PO NUMBER GENERATION
		// Format: PO-JobNumber-01, PO-JobNumber-02
		// =========================================================
		public string GenerateNextPONumber(int jobNumber)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT PONumber
FROM PurchaseOrders
WHERE JobNumber = $jobNumber;";
			cmd.Parameters.AddWithValue("$jobNumber", jobNumber);

			var existing = new List<string>();

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				existing.Add(reader.GetString(0));
			}

			int highestSuffix = 0;

			foreach (var po in existing)
			{
				var parts = po.Split('-');
				if (parts.Length >= 3 && int.TryParse(parts[2], out int suffix))
				{
					if (suffix > highestSuffix)
						highestSuffix = suffix;
				}
			}

			int next = highestSuffix + 1;

			return $"PO-{jobNumber}-{next:00}";
		}

		// =========================================================
		// SAVE PURCHASE ORDER
		// =========================================================
		public void Save(PurchaseOrder po)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var tx = connection.BeginTransaction();

			try
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.Transaction = tx;

					cmd.CommandText = @"
INSERT INTO PurchaseOrders
(Id, ProjectId, JobNumber, PONumber, SupplierName, CreatedDate, Status, TotalAmount)
VALUES
($id, $projectId, $jobNumber, $poNumber, $supplier, $created, $status, $total);";

					cmd.Parameters.AddWithValue("$id", po.Id.ToString());
					cmd.Parameters.AddWithValue("$projectId", po.ProjectId.ToString());
					cmd.Parameters.AddWithValue("$jobNumber", po.JobNumber);
					cmd.Parameters.AddWithValue("$poNumber", po.PONumber);
					cmd.Parameters.AddWithValue("$supplier", po.SupplierName ?? "");
					cmd.Parameters.AddWithValue("$created", po.CreatedDate.ToString("o"));
					cmd.Parameters.AddWithValue("$status", po.Status);
					cmd.Parameters.AddWithValue("$total", po.TotalAmount);

					cmd.ExecuteNonQuery();
				}

				foreach (var line in po.Lines)
				{
					using var lineCmd = connection.CreateCommand();
					lineCmd.Transaction = tx;

					lineCmd.CommandText = @"
INSERT INTO PurchaseOrderLines
(Id, PurchaseOrderId, StockItemId, ItemCode, Description, Quantity, UnitCost, LineTotal)
VALUES
($id, $poId, $stockId, $code, $desc, $qty, $cost, $total);";

					lineCmd.Parameters.AddWithValue("$id", line.Id.ToString());
					lineCmd.Parameters.AddWithValue("$poId", po.Id.ToString());
					lineCmd.Parameters.AddWithValue("$stockId", line.StockItemId.ToString());
					lineCmd.Parameters.AddWithValue("$code", line.ItemCode ?? "");
					lineCmd.Parameters.AddWithValue("$desc", line.Description ?? "");
					lineCmd.Parameters.AddWithValue("$qty", line.Quantity);
					lineCmd.Parameters.AddWithValue("$cost", line.UnitCost);
					lineCmd.Parameters.AddWithValue("$total", line.LineTotal);

					lineCmd.ExecuteNonQuery();
				}

				tx.Commit();
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}

		// =========================================================
		// LOAD POs BY PROJECT
		// =========================================================
		public List<PurchaseOrder> GetByProject(Guid projectId)
		{
			var list = new List<PurchaseOrder>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, JobNumber, PONumber, SupplierName, CreatedDate, Status, TotalAmount
FROM PurchaseOrders
WHERE ProjectId = $projectId
ORDER BY CreatedDate DESC;";
			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
                Guid.TryParse(reader[0]?.ToString(), out var id);

                DateTime.TryParse(reader[4]?.ToString(), out var createdDate);
                Guid.TryParse(reader[0]?.ToString(), out var poId);

                list.Add(new PurchaseOrder
				{
					Id = id,
					ProjectId = projectId,
					JobNumber = reader.GetInt32(1),
					PONumber = reader.GetString(2),
					SupplierName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CreatedDate = createdDate,
                    Status = reader.GetString(5),
					TotalAmount = reader.GetDecimal(6),
                    Lines = GetLines(poId)
                });
			}

			return list;
		}

		private List<PurchaseOrderLine> GetLines(Guid poId)
		{
			var lines = new List<PurchaseOrderLine>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Id, StockItemId, ItemCode, Description, Quantity, UnitCost, LineTotal
FROM PurchaseOrderLines
WHERE PurchaseOrderId = $poId;";
			cmd.Parameters.AddWithValue("$poId", poId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
                Guid.TryParse(reader[0]?.ToString(), out var id);
                Guid.TryParse(reader[1]?.ToString(), out var stockItemId);

                lines.Add(new PurchaseOrderLine
				{
					Id = id,
					PurchaseOrderId = poId,
					StockItemId = stockItemId,
					ItemCode = reader.IsDBNull(2) ? "" : reader.GetString(2),
					Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
					Quantity = reader.GetInt32(4),
					UnitCost = reader.GetDecimal(5),
					LineTotal = reader.GetDecimal(6)
				});
			}

			return lines;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderExecutionService
	{
		private static bool _isStarting = false;
		private readonly WorkOrderRepository _repository;
		private readonly WorkOrderMaterialRepository _materialRepo;
		private readonly MaterialValidator _materialValidator;
		private readonly BlockReasonEngine _blockEngine;
		private readonly StockRepository _stockRepo;

		public WorkOrderExecutionService(
			WorkOrderRepository repo,
			WorkOrderMaterialRepository materialRepo,
			MaterialValidator validator,
			StockRepository stockRepo)
		{
			_repository = repo;
			_materialRepo = materialRepo;
			_materialValidator = validator;
			_stockRepo = stockRepo;

			_blockEngine = new BlockReasonEngine();
		}

		public void StartWorkOrder(Guid workOrderId)
		{
			if (_isStarting)
			{
				Debug.WriteLine("⚠ BLOCKED duplicate start");
				return;
			}

			_isStarting = true;

			try
			{
				Debug.WriteLine("🔥 ENTERED StartWorkOrder()");
				Debug.WriteLine($"🚀 StartWorkOrder CALLED: {workOrderId}");

				var workOrder = _repository.GetById(workOrderId)
					?? throw new Exception("Work order not found");

				if (workOrder.Status == WorkOrderStatus.InProduction)
				{
					Debug.WriteLine($"🛑 HARD BLOCK: Already started {workOrder.WorkOrderNumber}");
					return;
				}

				var materials = _materialRepo.GetByWorkOrderId(workOrder.Id)
					?? new List<WorkOrderMaterial>();
				if (materials == null || !materials.Any())
					return;

				Debug.WriteLine($"🔥 MATERIAL COUNT: {materials.Count}");

				foreach (var m in materials)
				{
					Debug.WriteLine($"➡ {m.ItemCode} | Qty: {m.RequiredQuantity}");
				}

				// Determine type
				workOrder.Type = materials.Any()
					? WorkOrderType.Production
					: WorkOrderType.Procurement;

				if (workOrder.Type == WorkOrderType.Production)
				{
					if (!materials.Any())
						throw new Exception("No materials linked");

					if (!_materialValidator.CanStart(workOrder, out var reason))
						throw new Exception(reason);

					if (!TryReserveMaterials(materials, workOrder))
						throw new Exception("Stock reservation failed");
				}

				// 🔥 COST CALC
				var transactions = _stockRepo
					.GetAllTransactions()
					.Where(t => t.Reference == workOrder.WorkOrderNumber && t.Type == "OUT");

				workOrder.MaterialCost = transactions.Sum(t => t.TransactionValue);

				using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
				connection.Open();

				using var cmd = connection.CreateCommand();
				cmd.CommandText = @"
UPDATE WorkOrders
SET Status = @Status,
    ActualStartTime = @StartTime,
    IsPaused = 0,
    MaterialCost = @MaterialCost
WHERE Id = @Id";

				cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
				cmd.Parameters.AddWithValue("@StartTime", DateTime.UtcNow.ToString("O"));
				cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.InProduction);
				cmd.Parameters.AddWithValue("@MaterialCost", workOrder.MaterialCost);

				cmd.ExecuteNonQuery();

				Debug.WriteLine("✅ Work order started");
			}
			finally
			{
				_isStarting = false;
			}
		}

		private bool TryReserveMaterials(List<WorkOrderMaterial> materials, WorkOrder workOrder)
		{
			try
			{
				foreach (var m in materials)
				{
					if (string.IsNullOrWhiteSpace(m.ItemCode))
					{
						Debug.WriteLine("⚠ Skipping material — empty ItemCode");
						continue;
					}

					var stock = _stockRepo.GetByItemCode(m.ItemCode);

					if (stock == null)
					{
						throw new Exception($"Stock item not found: {m.ItemCode}");
					}

					if (stock.Quantity < m.RequiredQuantity)
					{
						throw new Exception($"Insufficient stock for: {m.ItemCode}");
					}

					// 🔥 Create transaction
					var tx = new StockTransaction
					{
						Id = Guid.NewGuid(),
						StockItemId = stock.Id,
						ItemCode = stock.ItemCode,
						ItemDescription = stock.Description,
						Quantity = (int)m.RequiredQuantity,
						Type = "OUT",
						TransactionDate = DateTime.UtcNow,
						UnitCost = stock.AverageUnitCost,
						ProjectId = workOrder.ProjectId,
						ProjectName = workOrder.ProjectName,
						Reference = workOrder.WorkOrderNumber
					};

					Debug.WriteLine($"📉 OUT: {tx.ItemCode} | {tx.Quantity}");

					_stockRepo.AddTransaction(tx);
				}

				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ MATERIAL RESERVATION ERROR:");
				Debug.WriteLine(ex.Message);
				return false;
			}
		}
		public void CompleteWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE WorkOrders
SET Status = @Status,
    ActualEndTime = @EndTime,
    CompletedOn = @EndTime,
    IsPaused = 0
WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@EndTime", DateTime.UtcNow.ToString("O"));
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Completed);

			cmd.ExecuteNonQuery();

			Debug.WriteLine("✅ Work order completed");
		}

		public void PauseWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE WorkOrders
SET IsPaused = 1,
    Status = @Status
WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Paused);

			cmd.ExecuteNonQuery();

			Debug.WriteLine("⏸ Work order paused");
		}

		public void CancelWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			var workOrder = _repository.GetById(workOrderId)
				?? throw new Exception("Work order not found");

			Debug.WriteLine($"⛔ Cancelling WO: {workOrder.WorkOrderNumber}");

			// 🔥 Reverse stock (ledger-based)
			var issued = _stockRepo.GetIssuedMaterials(workOrder.ProjectId);

			foreach (var tx in issued)
			{
				var returnTx = new StockTransaction
				{
					Id = Guid.NewGuid(),
					StockItemId = tx.StockItemId,
					ItemCode = tx.ItemCode,
					ItemDescription = tx.ItemDescription,
					Quantity = tx.Quantity,
					Type = "RET",
					TransactionDate = DateTime.UtcNow,
					UnitCost = tx.UnitCost,
					ProjectId = workOrder.ProjectId,
					ProjectName = workOrder.ProjectName,
					Reference = $"{workOrder.WorkOrderNumber}-REVERSAL"
				};

				if (string.IsNullOrWhiteSpace(tx.ItemCode))
				{
					Debug.WriteLine("⚠ Skipping invalid transaction (empty ItemCode)");
					continue;
				}

				Debug.WriteLine($"↩ RETURN: {returnTx.ItemCode} | {returnTx.Quantity}");

				_stockRepo.AddTransaction(returnTx);
			}

			// 🔥 Update status
			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE WorkOrders
SET Status = @Status,
    IsPaused = 0
WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Cancelled);

			cmd.ExecuteNonQuery();

			Debug.WriteLine("❌ Work order cancelled");
		}
	}

}
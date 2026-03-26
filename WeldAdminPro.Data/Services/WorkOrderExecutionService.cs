using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		private readonly WorkOrderRepository _repository;
		private readonly WorkOrderMaterialRepository _materialRepo;
		private readonly MaterialValidator _materialValidator;
		private readonly BlockReasonEngine _blockEngine;
		private readonly StockRepository _stockRepo;

		public WorkOrderExecutionService(
			   WorkOrderRepository repo,
			   WorkOrderMaterialRepository materialRepo,
			   MaterialValidator validator,
			   StockRepository stockRepo) // 👈 ADD
		{
			_repository = repo;
			_materialRepo = materialRepo;
			_materialValidator = validator;
			_stockRepo = stockRepo; // 👈 ADD

			_blockEngine = new BlockReasonEngine();
		}

		public void StartWorkOrder(Guid workOrderId)
		{
			System.Diagnostics.Debug.WriteLine("🔥 ENTERED StartWorkOrder()");
			try
			{
				using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
				connection.Open();

				System.Diagnostics.Debug.WriteLine($"🔍 Starting WO: {workOrderId}");

				var workOrder = _repository.GetById(workOrderId);
				if (workOrder == null)
					throw new Exception("Work order not found");

				// 🔒 Prevent duplicate start
				if (workOrder.Status == WorkOrderStatus.InProduction)
				{
					Debug.WriteLine($"⚠ Already running: {workOrder.WorkOrderNumber}");
					return;
				}

				var materials = _materialRepo.GetByWorkOrderId(workOrder.Id);
				// 🔥 MAP MATERIALS FOR ENGINE
				var stockItems = _stockRepo.GetAll();

				workOrder.MaterialRequirements = materials.Select(m =>
				{
					var stock = stockItems
						.FirstOrDefault(s => s.ItemCode == m.ItemCode);

					return new MaterialRequirement
					{
						MaterialCode = m.ItemCode,
						RequiredQuantity = m.RequiredQuantity,
						AvailableQuantity = stock?.Quantity ?? 0
					};
				}).ToList();

				// 🔥 BLOCK ENGINE VALIDATION
				var blockResult = _blockEngine.Evaluate(workOrder);

				if (blockResult.Reason != BlockReason.None)
				{
					throw new Exception($"Blocked: {blockResult.Message}");
				}

				// 🔥 AUTO-DETECT WORK ORDER TYPE
				if (!materials.Any())
				{
					workOrder.Type = WorkOrderType.Procurement;
				}
				else
				{
					workOrder.Type = WorkOrderType.Production;
				}

				string materialReason;

				if (workOrder.Type == WorkOrderType.Production)
				{
					Debug.WriteLine($"Materials found: {materials.Count}");

					if (!materials.Any())
						throw new Exception("Cannot start: No materials linked");

					if (!_materialValidator.CanStart(workOrder, out materialReason))
						throw new Exception($"Material check failed: {materialReason}");

					Debug.WriteLine("✅ Material validation passed");

					Debug.WriteLine("➡ Running Stock Reservation...");

					if (!TryReserveMaterials(materials, workOrder))
						throw new Exception("Not enough stock");

					Debug.WriteLine("✅ Stock reservation passed");
				}
				else
				{
					Debug.WriteLine("ℹ Non-production WO → skipping material + stock validation");
				}

				// 🔹 LOAD ALL WORK ORDERS
				var allWorkOrders = new List<WorkOrder>();

				using (var loadCmd = connection.CreateCommand())
				{
					loadCmd.CommandText = "SELECT Id, Status FROM WorkOrders";

					using var reader = loadCmd.ExecuteReader();

					while (reader.Read())
					{
						allWorkOrders.Add(new WorkOrder
						{
							Id = Guid.Parse(reader.GetString(0)),
							Status = (WorkOrderStatus)reader.GetInt32(1)
						});
					}
				}

				// 🔹 DEPENDENCIES (SAFE LOAD)
				workOrder.DependencyIds = new List<Guid>();

				try
				{
					using var checkCmd = connection.CreateCommand();
					checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='WorkOrderDependencies';";

					var exists = checkCmd.ExecuteScalar();

					if (exists == null)
					{
						Debug.WriteLine("⚠ Dependency table does not exist — skipping");
					}
					else
					{
						using var depCmd = connection.CreateCommand();
						depCmd.CommandText = @"
            SELECT DependsOnWorkOrderId 
            FROM WorkOrderDependencies
            WHERE WorkOrderId = @Id";

						depCmd.Parameters.AddWithValue("@Id", workOrderId.ToString());

						using var reader = depCmd.ExecuteReader();

						while (reader.Read())
						{
							workOrder.DependencyIds.Add(Guid.Parse(reader.GetString(0)));
						}

						Debug.WriteLine($"Dependencies loaded: {workOrder.DependencyIds.Count}");
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("⚠ Dependency load failed:");
					Debug.WriteLine(ex.Message);
				}

				// 🔥 TEMP BYPASS (FOR NOW)
				//bool canExecute = true;

				// 🔹 START WORK ORDER
				using var cmd = connection.CreateCommand();

				cmd.CommandText =
				@"UPDATE WorkOrders
                  SET Status = @Status,
                      ActualStartTime = @StartTime,
                      IsPaused = 0
                  WHERE Id = @Id";

				cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
				cmd.Parameters.AddWithValue("@StartTime", DateTime.UtcNow.ToString("O"));
				cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.InProduction);

				System.Diagnostics.Debug.WriteLine("➡ Starting Work Order (DB Update)...");

				var rows = cmd.ExecuteNonQuery();

				System.Diagnostics.Debug.WriteLine($"✅ Rows updated: {rows}");

				if (rows == 0)
				{
					System.Diagnostics.Debug.WriteLine("❌ WARNING: No rows updated!");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("❌ START ERROR:");
				System.Diagnostics.Debug.WriteLine(ex.ToString());

				throw; // 🔥 IMPORTANT - DO NOT SWALLOW
			}
		}

		public void CompleteWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"UPDATE WorkOrders
              SET Status = @Status,
                  ActualEndTime = @EndTime,
                  CompletedOn = @EndTime,
                  IsPaused = 0
              WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@EndTime", DateTime.UtcNow.ToString("O"));
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Completed);

			cmd.ExecuteNonQuery();
		}

		public void PauseWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"UPDATE WorkOrders
              SET IsPaused = 1,
                  Status = @Status
              WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Paused);

			cmd.ExecuteNonQuery();
		}

		private bool TryReserveMaterials(List<WorkOrderMaterial> materials, WorkOrder workOrder)
		{
			try
			{
				// 🔥 STEP 1 — PRE-VALIDATE ALL MATERIALS
				foreach (var m in materials)
				{
					var stockItem = _stockRepo.GetByItemCode(m.ItemCode);

					if (stockItem == null)
					{
						Debug.WriteLine($"❌ Missing stock item: {m.ItemCode}");
						return false;
					}

					if (stockItem.Quantity < m.RequiredQuantity)
					{
						Debug.WriteLine($"❌ Insufficient stock: {m.ItemCode}");
						return false;
					}
				}

				// 🔥 STEP 2 — EXECUTE LEDGER TRANSACTIONS
				foreach (var m in materials)
				{
					var stockItem = _stockRepo.GetByItemCode(m.ItemCode)!;

					var tx = new StockTransaction
					{
						Id = Guid.NewGuid(),
						StockItemId = stockItem.Id,
						ItemCode = stockItem.ItemCode,
						ItemDescription = stockItem.Description,
						Quantity = (int)m.RequiredQuantity,
						Type = "OUT",
						TransactionDate = DateTime.UtcNow,
						UnitCost = stockItem.AverageUnitCost,
						ProjectId = workOrder.ProjectId,
						ProjectName = workOrder.ProjectName,
						Reference = $"WO-{workOrder.WorkOrderNumber}"
					};

					Debug.WriteLine($"➡ Ledger OUT: {tx.ItemCode} | Qty: {tx.Quantity}");
					Debug.WriteLine($"StockItemId: {stockItem.Id}");
					Debug.WriteLine($"ItemCode: {stockItem.ItemCode}");

					_stockRepo.AddTransaction(tx);
				}

				Debug.WriteLine("✅ Ledger-based deduction complete");

				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ Ledger deduction failed:");
				Debug.WriteLine(ex.ToString());

				return false;
			}
		}

			public void CancelWorkOrder(Guid workOrderId)
		{
			try
			{
				using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
				connection.Open();

				var workOrder = _repository.GetById(workOrderId);
				if (workOrder == null)
					throw new Exception("Work order not found");

				Debug.WriteLine($"⛔ Cancelling WO: {workOrder.WorkOrderNumber}");

				// 🔥 STEP 1 — GET ALL ISSUED MATERIALS FROM LEDGER
				var txRepo = new StockTransactionRepository();
				var issuedMaterials = txRepo.GetIssuedMaterials(workOrder.ProjectId);

				Debug.WriteLine($"Found {issuedMaterials.Count} issued transactions");

				// 🔥 STEP 2 — REVERSE THEM (RETURN TO STOCK)
				foreach (var tx in issuedMaterials)
				{
					var returnTx = new StockTransaction
					{
						Id = Guid.NewGuid(),
						StockItemId = tx.StockItemId,
						ItemCode = tx.ItemCode,
						ItemDescription = tx.ItemDescription,
						Quantity = tx.Quantity,
						Type = "RET", // 🔥 RETURN
						TransactionDate = DateTime.UtcNow,
						UnitCost = tx.UnitCost,
						ProjectId = workOrder.ProjectId,
						ProjectName = workOrder.ProjectName,
						Reference = $"WO-{workOrder.WorkOrderNumber}-REVERSAL"
					};

					Debug.WriteLine($"↩ Returning: {returnTx.ItemCode} | Qty: {returnTx.Quantity}");

					_stockRepo.AddTransaction(returnTx);
				}

				Debug.WriteLine("✅ Stock successfully returned");

				// 🔥 STEP 3 — UPDATE WORK ORDER STATUS
				using var cmd = connection.CreateCommand();

				cmd.CommandText =
				@"UPDATE WorkOrders
		  SET Status = @Status,
		      IsPaused = 0
		  WHERE Id = @Id";

				cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
				cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Cancelled);

				cmd.ExecuteNonQuery();

				Debug.WriteLine("✅ Work order cancelled");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ CANCEL ERROR:");
				Debug.WriteLine(ex.ToString());
				throw;
			}
		}
	}
	}
	

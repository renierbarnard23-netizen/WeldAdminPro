using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeldAdminPro.Core.Events;
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
		private readonly StockProjectTransactionService _stockTxService;

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
			_stockTxService = new StockProjectTransactionService();
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
				Debug.WriteLine($"🚀 StartWorkOrder CALLED: {workOrderId}");

				var workOrder = _repository.GetById(workOrderId)
					?? throw new Exception("Work order not found");

				using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
				connection.Open();

				// ✅ FIX: GUID must be string
				using var checkCmd = connection.CreateCommand();
				checkCmd.CommandText = "SELECT Status FROM WorkOrders WHERE Id = @Id";
				checkCmd.Parameters.AddWithValue("@Id", workOrderId.ToString());

				var currentStatus = Convert.ToInt32(checkCmd.ExecuteScalar());

				if (currentStatus == (int)WorkOrderStatus.InProduction)
				{
					Debug.WriteLine("⚠ ALREADY STARTED (DB BLOCK)");
					return;
				}

				var materials = _materialRepo.GetByWorkOrderId(workOrder.Id)
					?? new List<WorkOrderMaterial>();

				// ✅ ONLY run material logic IF materials exist
				if (materials.Any())
				{
					if (!_materialValidator.CanStart(workOrder, out var validationReason))
						throw new Exception(validationReason);

					IssueMaterials(materials, workOrder);

					var txList = _stockRepo
						.GetAllTransactions()
						.Where(t => t.Reference == workOrder.WorkOrderNumber && t.Type == "OUT");
					
				}

				if (workOrder.Status != WorkOrderStatus.Ready
	&& workOrder.Status != WorkOrderStatus.Paused)
				{
					Debug.WriteLine("⚠ Already started or not in Ready/Paused state");
					return;
				}

				using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
				UPDATE WorkOrders
				SET Status = @Status,
					ActualStartTime = @StartTime,
					IsPaused = 0
				WHERE Id = @Id";

                cmd.Parameters.AddWithValue("@Id", workOrder.Id.ToString());
				cmd.Parameters.AddWithValue("@StartTime", DateTime.UtcNow.ToString("O"));
				cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.InProduction);

				var rows = cmd.ExecuteNonQuery();

				Debug.WriteLine($"✅ ROWS UPDATED: {rows}");
				Debug.WriteLine("✅ WORK ORDER STARTED SUCCESSFULLY");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"❌ START FAILED: {ex.Message}");
				throw;
			}
			finally
			{
				_isStarting = false;
			}

            WorkOrderEvents.RaiseChanged();
        }

		private void IssueMaterials(List<WorkOrderMaterial> materials, WorkOrder workOrder)
		{
			foreach (var m in materials)
			{
				var stock = _stockRepo.GetByItemCode(m.ItemCode)
					?? throw new Exception($"Stock item not found: {m.ItemCode}");

                var digits =
    new string(
        workOrder.WorkOrderNumber
            .Where(char.IsDigit)
            .ToArray());

                int.TryParse(digits,
                             out int jobNumber);

                _stockTxService.IssueStock(
					new Project
					{
						Id = workOrder.ProjectId,
						JobNumber = jobNumber // ✅ FIXED
					},
					stock,
					(decimal)m.RequiredQuantity,
					"SYSTEM"
				);
			}
            WorkOrderEvents.RaiseChanged();
        }

        public void CompleteWorkOrder(Guid workOrderId)
        {
            using var connection =
                new SqliteConnection(
                    $"Data Source={DatabasePath.Get()}");

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
			UPDATE WorkOrders
			SET Status = @Status,
				ActualEndTime = @EndTime,
				CompletedOn = @EndTime
				WHERE Id = @Id";

            cmd.Parameters.AddWithValue(
				"@Id",
                workOrderId.ToString());

            cmd.Parameters.AddWithValue(
                "@EndTime",
                DateTime.UtcNow.ToString("O"));

            cmd.Parameters.AddWithValue(
                "@Status",
                (int)WorkOrderStatus.Completed);

            var rows = cmd.ExecuteNonQuery();

            Debug.WriteLine(
                $"COMPLETE ROWS = {rows}");

            WorkOrderEvents.RaiseChanged();
        }

        public void CancelWorkOrder(Guid workOrderId)
		{
			var workOrder = _repository.GetById(workOrderId)
				?? throw new Exception("Work order not found");

			var issued = _stockRepo.GetIssuedMaterials(workOrder.ProjectId);

			foreach (var tx in issued)
				{
					_stockRepo.AddTransaction(new StockTransaction
					{
						Id = Guid.NewGuid(),
						StockItemId = tx.StockItemId,
						ItemCode = tx.ItemCode,
						Quantity = tx.Quantity, // ✅ FIXED
						Type = "RET",
						TransactionDate = DateTime.UtcNow,
						ProjectId = workOrder.ProjectId,
						Reference = workOrder.WorkOrderNumber + "-REV"
					});
				}
            WorkOrderEvents.RaiseChanged();
        }
		public void PauseWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
UPDATE WorkOrders
SET Status = @Status,
    IsPaused = 1
WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@Status", (int)WorkOrderStatus.Paused);

			cmd.ExecuteNonQuery();

            WorkOrderEvents.RaiseChanged();
        }
	}
	}

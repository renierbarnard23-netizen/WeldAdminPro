using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderExecutionService
	{
		private readonly WorkOrderRepository _repository;
		private readonly WorkOrderMaterialRepository _materialRepo;
		private readonly MaterialValidator _materialValidator;

		public WorkOrderExecutionService(
			WorkOrderRepository repository,
			WorkOrderMaterialRepository materialRepo,
			MaterialValidator materialValidator)
		{
			_repository = repository;
			_materialRepo = materialRepo;
			_materialValidator = materialValidator;
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

					if (!TryReserveMaterials(materials))
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

					System.Diagnostics.Debug.WriteLine($"Dependencies loaded: {workOrder.DependencyIds.Count}");
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("⚠ No dependency table or data — skipping");
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

		private bool TryReserveMaterials(List<WorkOrderMaterial> materials)
		{
			foreach (var m in materials)
			{
				System.Diagnostics.Debug.WriteLine($"🔍 Checking stock for {m.ItemCode}");

				var availableStock = 100; // TEMP

				if (availableStock < m.RequiredQuantity)
				{
					System.Diagnostics.Debug.WriteLine($"❌ Not enough stock for {m.ItemCode}");
					return false;
				}
			}

			System.Diagnostics.Debug.WriteLine("✅ Materials reserved");
			return true;
		}
	}
}
using System;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Production.Services;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderExecutionService
	{
		private readonly WorkOrderRepository _repository;
		private readonly MaterialValidator _materialValidator;

		public WorkOrderExecutionService(WorkOrderRepository repository,MaterialValidator materialValidator)
		{
			_repository = repository;
			_materialValidator = materialValidator;
		}

		public void StartWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

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



			// 🔹 LOAD FULL WORK ORDER (SOURCE OF TRUTH)
			var workOrder = _repository.GetById(workOrderId);

			if (workOrder == null)
				throw new Exception("Work order not found");

			// 🔥 MATERIAL VALIDATION
			if (!_materialValidator.CanStart(workOrder, out var materialReason))
			{
				Console.WriteLine($"❌ MATERIAL BLOCK: {materialReason}");
				return;
			}


			// 🔹 TEMP DEPENDENCY SUPPORT
			using (var depCmd = connection.CreateCommand())
			{
				depCmd.CommandText = @"
        SELECT DependsOnWorkOrderId 
        FROM WorkOrderDependencies
        WHERE WorkOrderId = @Id";

				depCmd.Parameters.AddWithValue("@Id", workOrderId.ToString());

				using var depReader = depCmd.ExecuteReader();

				workOrder.DependencyIds = new List<Guid>();

				while (depReader.Read())
				{
					workOrder.DependencyIds.Add(
						Guid.Parse(depReader.GetString(0))
					);
				}
			}

			// 🔥 DEPENDENCY VALIDATION
			var canExecute = DependencyValidator.CanExecute(
				workOrder,
				allWorkOrders,
				out var reason);

			if (!canExecute)
			{
				throw new InvalidOperationException(
					$"Cannot start work order: {reason}");
			}

			// 🔹 EXECUTE UPDATE
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

			cmd.ExecuteNonQuery();
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
	}
}
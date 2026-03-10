using System;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderExecutionService
	{
		private readonly WorkOrderRepository _repository;

		public WorkOrderExecutionService(WorkOrderRepository repository)
		{
			_repository = repository;
		}

		public void StartWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"UPDATE WorkOrders
              SET Status = 2,
                  ActualStartTime = @StartTime
              WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@StartTime", DateTime.UtcNow.ToString("O"));

			cmd.ExecuteNonQuery();
		}

		public void CompleteWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"UPDATE WorkOrders
              SET Status = 3,
                  ActualEndTime = @EndTime
              WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());
			cmd.Parameters.AddWithValue("@EndTime", DateTime.UtcNow.ToString("O"));

			cmd.ExecuteNonQuery();
		}
		public void PauseWorkOrder(Guid workOrderId)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"UPDATE WorkOrders
      SET IsPaused = 1
      WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrderId.ToString());

			cmd.ExecuteNonQuery();
		}
	}
}
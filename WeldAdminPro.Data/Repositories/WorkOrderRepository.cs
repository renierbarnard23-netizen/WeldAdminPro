using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class WorkOrderRepository
	{
		private readonly string _connectionString;

		public WorkOrderRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureTable();
		}

		private void EnsureTable()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
				CREATE TABLE IF NOT EXISTS WorkOrders (
					Id TEXT PRIMARY KEY,
					ProjectId TEXT NOT NULL,
					WorkOrderNumber TEXT NOT NULL,
					Description TEXT NOT NULL,
					Status INTEGER NOT NULL,
					CreatedOn TEXT NOT NULL,
					CompletedOn TEXT
					PlannedStartDate TEXT,
					DueDate TEXT,
					Priority INTEGER
			);

				CREATE TABLE IF NOT EXISTS WorkOrderSettings (
					Key TEXT PRIMARY KEY,
					Value TEXT NOT NULL
			);";

			cmd.ExecuteNonQuery();
		}

		public void UpdatePriority(string workOrderNumber, int priority)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE WorkOrders
SET Priority = @Priority
WHERE WorkOrderNumber = @WorkOrderNumber";

			cmd.Parameters.AddWithValue("@Priority", priority);
			cmd.Parameters.AddWithValue("@WorkOrderNumber", workOrderNumber);

			cmd.ExecuteNonQuery();
		}

		public IEnumerable<WorkOrder> GetByProject(Guid projectId)
		{
			var list = new List<WorkOrder>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM WorkOrders WHERE ProjectId = @ProjectId";
			cmd.Parameters.AddWithValue("@ProjectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new WorkOrder
				{
					Id = Guid.Parse(reader["Id"].ToString()!),
					ProjectId = Guid.Parse(reader["ProjectId"].ToString()!),
					WorkOrderNumber = reader["WorkOrderNumber"].ToString()!,
					Description = reader["Description"].ToString()!,
					Status = (WorkOrderStatus)Convert.ToInt32(reader["Status"]),
					CreatedOn = DateTime.Parse(reader["CreatedOn"].ToString()!),
					CompletedOn = reader["CompletedOn"] == DBNull.Value
						? null
						: DateTime.Parse(reader["CompletedOn"].ToString()!)
				});
			}

			return list;
		}


		public IEnumerable<WorkOrder> GetAll()
		{
			var list = new List<WorkOrder>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM WorkOrders ORDER BY CreatedOn DESC";

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new WorkOrder
				{
					Id = Guid.Parse(reader["Id"].ToString()!),
					ProjectId = Guid.Parse(reader["ProjectId"].ToString()!),
					WorkOrderNumber = reader["WorkOrderNumber"].ToString()!,
					Description = reader["Description"].ToString()!,
					Status = (WorkOrderStatus)Convert.ToInt32(reader["Status"]),
					CreatedOn = DateTime.Parse(reader["CreatedOn"].ToString()!),
					CompletedOn = reader["CompletedOn"] == DBNull.Value
						? null
						: DateTime.Parse(reader["CompletedOn"].ToString()!)
				});
			}

			return list;
		}
		public string GetNextWorkOrderNumber()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Value FROM WorkOrderSettings WHERE Key='NextWorkOrderNumber'";

			var result = cmd.ExecuteScalar();

			int nextNumber;

			if (result == null)
			{
				nextNumber = 1;

				using var insert = connection.CreateCommand();
				insert.CommandText = @"
INSERT INTO WorkOrderSettings (Key, Value)
VALUES ('NextWorkOrderNumber', '2')";
				insert.ExecuteNonQuery();
			}
			else
			{
				nextNumber = int.Parse(result.ToString());

				using var update = connection.CreateCommand();
				update.CommandText = @"
UPDATE WorkOrderSettings
SET Value=@Value
WHERE Key='NextWorkOrderNumber'";
				update.Parameters.AddWithValue("@Value", (nextNumber + 1).ToString());
				update.ExecuteNonQuery();
			}

			return $"WO-{nextNumber:000}";
		}

		public void Add(WorkOrder workOrder)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			if (string.IsNullOrWhiteSpace(workOrder.WorkOrderNumber))
			{
				workOrder.WorkOrderNumber = GetNextWorkOrderNumber();
			}


			cmd.CommandText = @"
INSERT INTO WorkOrders (
    Id,
    ProjectId,
    WorkOrderNumber,
    Description,
    Status,
    CreatedOn,
    CompletedOn
)
VALUES (
    @Id,
    @ProjectId,
    @WorkOrderNumber,
    @Description,
    @Status,
    @CreatedOn,
    @CompletedOn
);";

			cmd.Parameters.AddWithValue("@Id", workOrder.Id.ToString());
			cmd.Parameters.AddWithValue("@ProjectId", workOrder.ProjectId.ToString());
			cmd.Parameters.AddWithValue("@WorkOrderNumber", workOrder.WorkOrderNumber);
			cmd.Parameters.AddWithValue("@Description", workOrder.Description);
			cmd.Parameters.AddWithValue("@Status", (int)workOrder.Status);
			cmd.Parameters.AddWithValue("@CreatedOn", workOrder.CreatedOn.ToString("O"));
			cmd.Parameters.AddWithValue("@CompletedOn",
				workOrder.CompletedOn.HasValue
					? workOrder.CompletedOn.Value.ToString("O")
					: DBNull.Value);

			cmd.ExecuteNonQuery();
		}
	}
}
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
			EnsureColumns();
		}

		public WorkOrder? GetById(Guid id)
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Id, Status FROM WorkOrders WHERE Id = @Id";
			cmd.Parameters.AddWithValue("@Id", id.ToString());

			using var reader = cmd.ExecuteReader();

			if (reader.Read())
			{
				return new WorkOrder
				{
					Id = Guid.Parse(reader.GetString(0)),
					Status = (WorkOrderStatus)reader.GetInt32(1)
				};
			}

			return null;
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
					StartDate TEXT,
					EstimatedHours REAL,
					Status INTEGER NOT NULL,
					CreatedOn TEXT NOT NULL,
					CompletedOn TEXT,
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

		private void EnsureColumns()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			void AddColumn(string column, string type)
			{
				using var cmd = connection.CreateCommand();

				cmd.CommandText = $@"
        SELECT COUNT(*) 
        FROM pragma_table_info('WorkOrders') 
        WHERE name='{column}'";

				var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

				if (!exists)
				{
					using var alter = connection.CreateCommand();
					alter.CommandText = $"ALTER TABLE WorkOrders ADD COLUMN {column} {type}";
					alter.ExecuteNonQuery();
				}
			}

			AddColumn("StartDate", "TEXT");
			AddColumn("EstimatedHours", "REAL");
			AddColumn("DueDate", "TEXT");
			AddColumn("Priority", "INTEGER");

			AddColumn("ActualStartTime", "TEXT");
			AddColumn("ActualEndTime", "TEXT");
			AddColumn("ActualHours", "REAL");
			AddColumn("IsPaused", "INTEGER");
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

		public void Update(WorkOrder workOrder)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
UPDATE WorkOrders
SET
    WorkOrderNumber = @WorkOrderNumber,
    ProjectId = @ProjectId,
    Description = @Description,
    StartDate = @StartDate,
    EstimatedHours = @EstimatedHours,
    DueDate = @DueDate,
    Status = @Status,
    ActualStartTime = @ActualStartTime,
    ActualEndTime = @ActualEndTime,
    ActualHours = @ActualHours,
    IsPaused = @IsPaused
WHERE Id = @Id";

			cmd.Parameters.AddWithValue("@Id", workOrder.Id.ToString());
			cmd.Parameters.AddWithValue("@ProjectId", workOrder.ProjectId.ToString());
			cmd.Parameters.AddWithValue("@WorkOrderNumber", workOrder.WorkOrderNumber);
			cmd.Parameters.AddWithValue("@Description", workOrder.Description);
			cmd.Parameters.AddWithValue("@StartDate", workOrder.StartDate.ToString("O"));
			cmd.Parameters.AddWithValue("@EstimatedHours", workOrder.EstimatedHours);
			cmd.Parameters.AddWithValue("@Status", (int)workOrder.Status);
			cmd.Parameters.AddWithValue("@ActualStartTime",
				workOrder.ActualStartTime.HasValue
					? workOrder.ActualStartTime.Value.ToString("O")
					: DBNull.Value);

			cmd.Parameters.AddWithValue("@ActualEndTime",
				workOrder.ActualEndTime.HasValue
					? workOrder.ActualEndTime.Value.ToString("O")
					: DBNull.Value);

			cmd.Parameters.AddWithValue("@ActualHours", workOrder.ActualHours);

			cmd.Parameters.AddWithValue("@IsPaused", workOrder.IsPaused ? 1 : 0);

			cmd.Parameters.AddWithValue("@DueDate", workOrder.DueDate.HasValue? workOrder.DueDate.Value.ToString("O"): DBNull.Value);

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
			cmd.CommandText = @"
				SELECT
				w.*,
				p.ProjectName
				FROM WorkOrders w
				LEFT JOIN Projects p ON p.Id = w.ProjectId
				ORDER BY w.CreatedOn DESC;";

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new WorkOrder
				{
					Id = Guid.Parse(reader["Id"].ToString()!),
					ProjectId = Guid.Parse(reader["ProjectId"].ToString()!),

					ProjectName = reader["ProjectName"] == DBNull.Value
					? ""
					: reader["ProjectName"].ToString()!,

					WorkOrderNumber = reader["WorkOrderNumber"].ToString()!,
					Description = reader["Description"].ToString()!,

					StartDate = reader["StartDate"] == DBNull.Value
						? DateTime.Today
						: DateTime.Parse(reader["StartDate"].ToString()!),

					EstimatedHours = reader["EstimatedHours"] == DBNull.Value
						? 8
						: Convert.ToDouble(reader["EstimatedHours"]),

					DueDate = reader["DueDate"] == DBNull.Value
						? DateTime.Today.AddDays(3)
						: DateTime.Parse(reader["DueDate"].ToString()!),

					Priority = reader["Priority"] == DBNull.Value
						? 1
						: Convert.ToInt32(reader["Priority"]),

					Status = (WorkOrderStatus)Convert.ToInt32(reader["Status"]),

					CreatedOn = DateTime.Parse(reader["CreatedOn"].ToString()!),

					CompletedOn = reader["CompletedOn"] == DBNull.Value
						? null
						: DateTime.Parse(reader["CompletedOn"].ToString()!),

					ActualStartTime = reader["ActualStartTime"] == DBNull.Value
						? null
						: DateTime.Parse(reader["ActualStartTime"].ToString()!),

					ActualEndTime = reader["ActualEndTime"] == DBNull.Value
						? null
						: DateTime.Parse(reader["ActualEndTime"].ToString()!),

					ActualHours = reader["ActualHours"] == DBNull.Value
						? 0
						: Convert.ToDouble(reader["ActualHours"]),

					IsPaused = reader["IsPaused"] == DBNull.Value
						? false
						: Convert.ToInt32(reader["IsPaused"]) == 1,
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
				nextNumber = int.Parse(result?.ToString() ?? "1");

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

			if (workOrder.ProjectId == Guid.Empty)
			{
				throw new InvalidOperationException(
					"WorkOrder cannot be saved without a ProjectId.");
			}

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
				DueDate,
				CompletedOn
			)
				VALUES (
				@Id,
				@ProjectId,
				@WorkOrderNumber,
				@Description,
				@Status,
				@CreatedOn,
				@DueDate,
				@CompletedOn
			);";

			cmd.Parameters.AddWithValue("@Id", workOrder.Id.ToString());
			cmd.Parameters.AddWithValue("@ProjectId", workOrder.ProjectId.ToString());
			cmd.Parameters.AddWithValue("@WorkOrderNumber", workOrder.WorkOrderNumber);
			cmd.Parameters.AddWithValue("@Description", workOrder.Description);
			cmd.Parameters.AddWithValue("@Status", (int)workOrder.Status);
			cmd.Parameters.AddWithValue("@CreatedOn", workOrder.CreatedOn.ToString("O"));
			cmd.Parameters.AddWithValue("@DueDate",
				workOrder.DueDate.HasValue
					? workOrder.DueDate.Value.ToString("O")
					: DBNull.Value);
			cmd.Parameters.AddWithValue("@CompletedOn",
				workOrder.CompletedOn.HasValue
					? workOrder.CompletedOn.Value.ToString("O")
					: DBNull.Value);

			cmd.ExecuteNonQuery();
		}
	}
}
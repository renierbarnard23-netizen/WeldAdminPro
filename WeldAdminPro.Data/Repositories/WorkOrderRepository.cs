using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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

        // =========================
        // TABLE SETUP
        // =========================

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
                    Priority INTEGER,
                    ActualStartTime TEXT,
                    ActualEndTime TEXT,
                    ActualHours REAL,
                    IsPaused INTEGER
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

        // =========================
        // GET BY ID
        // =========================

        public WorkOrder? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Status FROM WorkOrders WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            Guid.TryParse(reader["Id"]?.ToString(), out var parsedId);

            return new WorkOrder
            {
                Id = parsedId,
                Status = (WorkOrderStatus)Convert.ToInt32(reader["Status"])
            };
        }

        // =========================
        // GET ALL
        // =========================

        public IEnumerable<WorkOrder> GetAll()
        {
            var list = new List<WorkOrder>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT w.*, p.ProjectName
                FROM WorkOrders w
                LEFT JOIN Projects p ON p.Id = w.ProjectId
                ORDER BY w.CreatedOn DESC;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Guid.TryParse(reader["Id"]?.ToString(), out var id);
                Guid.TryParse(reader["ProjectId"]?.ToString(), out var projectId);

                DateTime.TryParse(reader["CreatedOn"]?.ToString(), out var createdOn);
                DateTime.TryParse(reader["DueDate"]?.ToString(), out var dueDate);
                DateTime.TryParse(reader["StartDate"]?.ToString(), out var startDate);
                DateTime.TryParse(reader["ActualStartTime"]?.ToString(), out var actualStart);
                DateTime.TryParse(reader["ActualEndTime"]?.ToString(), out var actualEnd);

                double.TryParse(reader["EstimatedHours"]?.ToString(), out var estimatedHours);
                int.TryParse(reader["Priority"]?.ToString(), out var priority);
                int.TryParse(reader["Status"]?.ToString(), out var status);
                double.TryParse(reader["ActualHours"]?.ToString(), out var actualHours);

                list.Add(new WorkOrder
                {
                    Id = id,
                    ProjectId = projectId,

                    ProjectName = reader["ProjectName"] == DBNull.Value
        ? ""
        : reader["ProjectName"].ToString()!,

                    WorkOrderNumber = reader["WorkOrderNumber"].ToString() ?? "",
                    Description = reader["Description"].ToString() ?? "",

                    StartDate = startDate == default ? DateTime.Today : startDate,
                    DueDate = dueDate == default ? DateTime.Today.AddDays(3) : dueDate,
                    CreatedOn = createdOn == default ? DateTime.Now : createdOn,

                    EstimatedHours = estimatedHours == 0 ? 8 : estimatedHours,
                    Priority = priority == 0 ? 1 : priority,
                    Status = (WorkOrderStatus)status,

                    CompletedOn = reader["CompletedOn"] == DBNull.Value
        ? null
        : DateTime.TryParse(reader["CompletedOn"]?.ToString(), out var comp)
            ? comp
            : null,

                    ActualStartTime = actualStart == default ? null : actualStart,
                    ActualEndTime = actualEnd == default ? null : actualEnd,

                    ActualHours = actualHours,

                    IsPaused = reader["IsPaused"] != DBNull.Value &&
               Convert.ToInt32(reader["IsPaused"]) == 1
                });
            }

            return list;
        }

        // =========================
        // ADD
        // =========================

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

        // =========================
        // UPDATE
        // =========================

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
                workOrder.ActualStartTime?.ToString("O") ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@ActualEndTime",
                workOrder.ActualEndTime?.ToString("O") ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@ActualHours", workOrder.ActualHours);
            cmd.Parameters.AddWithValue("@IsPaused", workOrder.IsPaused ? 1 : 0);

            cmd.Parameters.AddWithValue("@DueDate",
                workOrder.DueDate?.ToString("O") ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // WORK ORDER NUMBER
        // =========================

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
                nextNumber = int.Parse(result.ToString()!);

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
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class InspectionTestPlanInspectionExecutionRepository
    {
        private readonly string _connectionString;

        public InspectionTestPlanInspectionExecutionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(InspectionTestPlanInspectionExecution execution)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO InspectionTestPlanInspectionExecutions
(
    Id,
    InspectionTestPlanItemId,
    WeldId,
    Status,
    Result,
    InspectionDate,
    InspectorName,
    Findings,
    Remarks,
    ExecutedBy,
    ExecutedOn,
    CompletedBy,
    CompletedOn
)
VALUES
(
    $id,
    $itemId,
    $weldId,
    $status,
    $result,
    $inspectionDate,
    $inspectorName,
    $findings,
    $remarks,
    $executedBy,
    $executedOn,
    $completedBy,
    $completedOn
);";

            cmd.Parameters.AddWithValue("$id", execution.Id.ToString());
            cmd.Parameters.AddWithValue("$itemId", execution.InspectionTestPlanItemId.ToString());
            cmd.Parameters.AddWithValue("$weldId", execution.WeldId.ToString());
            cmd.Parameters.AddWithValue("$status", (int)execution.Status);
            cmd.Parameters.AddWithValue("$result", (int)execution.Result);
            cmd.Parameters.AddWithValue(
                "$inspectionDate",
                execution.InspectionDate.HasValue
                    ? execution.InspectionDate.Value.ToString("O")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$inspectorName", execution.InspectorName ?? string.Empty);
            cmd.Parameters.AddWithValue("$findings", execution.Findings ?? string.Empty);
            cmd.Parameters.AddWithValue("$remarks", execution.Remarks ?? string.Empty);
            cmd.Parameters.AddWithValue("$executedBy", execution.ExecutedBy ?? string.Empty);
            cmd.Parameters.AddWithValue("$executedOn", execution.ExecutedOn.ToString("O"));
            cmd.Parameters.AddWithValue(
                "$completedBy",
                execution.CompletedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue(
                "$completedOn",
                execution.CompletedOn.HasValue
                    ? execution.CompletedOn.Value.ToString("O")
                    : (object)DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public InspectionTestPlanInspectionExecution? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    WeldId,
    Status,
    Result,
    InspectionDate,
    InspectorName,
    Findings,
    Remarks,
    ExecutedBy,
    ExecutedOn,
    CompletedBy,
    CompletedOn
FROM InspectionTestPlanInspectionExecutions
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();

            return reader.Read()
                ? Map(reader)
                : null;
        }

        public List<InspectionTestPlanInspectionExecution> GetByItem(
            Guid inspectionTestPlanItemId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    WeldId,
    Status,
    Result,
    InspectionDate,
    InspectorName,
    Findings,
    Remarks,
    ExecutedBy,
    ExecutedOn,
    CompletedBy,
    CompletedOn
FROM InspectionTestPlanInspectionExecutions
WHERE InspectionTestPlanItemId = $itemId
ORDER BY ExecutedOn DESC, Id;";

            cmd.Parameters.AddWithValue(
                "$itemId",
                inspectionTestPlanItemId.ToString());

            using var reader = cmd.ExecuteReader();

            var results = new List<InspectionTestPlanInspectionExecution>();

            while (reader.Read())
            {
                results.Add(Map(reader));
            }

            return results;
        }

        public List<InspectionTestPlanInspectionExecution> GetByWeld(Guid weldId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    WeldId,
    Status,
    Result,
    InspectionDate,
    InspectorName,
    Findings,
    Remarks,
    ExecutedBy,
    ExecutedOn,
    CompletedBy,
    CompletedOn
FROM InspectionTestPlanInspectionExecutions
WHERE WeldId = $weldId
ORDER BY ExecutedOn DESC, Id;";

            cmd.Parameters.AddWithValue("$weldId", weldId.ToString());

            using var reader = cmd.ExecuteReader();

            var results = new List<InspectionTestPlanInspectionExecution>();

            while (reader.Read())
            {
                results.Add(Map(reader));
            }

            return results;
        }

        public void Update(InspectionTestPlanInspectionExecution execution)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE InspectionTestPlanInspectionExecutions
SET
    Status = $status,
    Result = $result,
    InspectionDate = $inspectionDate,
    InspectorName = $inspectorName,
    Findings = $findings,
    Remarks = $remarks,
    CompletedBy = $completedBy,
    CompletedOn = $completedOn
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$status", (int)execution.Status);
            cmd.Parameters.AddWithValue("$result", (int)execution.Result);
            cmd.Parameters.AddWithValue(
                "$inspectionDate",
                execution.InspectionDate.HasValue
                    ? execution.InspectionDate.Value.ToString("O")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$inspectorName", execution.InspectorName ?? string.Empty);
            cmd.Parameters.AddWithValue("$findings", execution.Findings ?? string.Empty);
            cmd.Parameters.AddWithValue("$remarks", execution.Remarks ?? string.Empty);
            cmd.Parameters.AddWithValue(
                "$completedBy",
                execution.CompletedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue(
                "$completedOn",
                execution.CompletedOn.HasValue
                    ? execution.CompletedOn.Value.ToString("O")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$id", execution.Id.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
DELETE FROM InspectionTestPlanInspectionExecutions
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        private static InspectionTestPlanInspectionExecution Map(
            SqliteDataReader reader)
        {
            return new InspectionTestPlanInspectionExecution
            {
                Id = Guid.Parse(reader.GetString(0)),
                InspectionTestPlanItemId = Guid.Parse(reader.GetString(1)),
                WeldId = Guid.Parse(reader.GetString(2)),
                Status = (InspectionTestPlanInspectionExecutionStatus)reader.GetInt32(3),
                Result = (InspectionTestPlanInspectionResult)reader.GetInt32(4),
                InspectionDate = reader.IsDBNull(5)
                    ? null
                    : DateTime.Parse(reader.GetString(5)),
                InspectorName = reader.GetString(6),
                Findings = reader.GetString(7),
                Remarks = reader.GetString(8),
                ExecutedBy = reader.GetString(9),
                ExecutedOn = DateTime.Parse(reader.GetString(10)),
                CompletedBy = reader.IsDBNull(11)
                    ? null
                    : reader.GetString(11),
                CompletedOn = reader.IsDBNull(12)
                    ? null
                    : DateTime.Parse(reader.GetString(12))
            };
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class InspectionTestPlanExecutionRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        public void Add(InspectionTestPlanExecution item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO InspectionTestPlanExecutions
(
    Id,
    InspectionTestPlanItemId,
    ExecutionType,
    ExecutionRecordId,
    LinkedOn,
    LinkedBy,
    Notes
)
VALUES
(
    $id,
    $itemId,
    $executionType,
    $executionRecordId,
    $linkedOn,
    $linkedBy,
    $notes
);";

            AddParameters(cmd, item);
            cmd.ExecuteNonQuery();
        }

        public InspectionTestPlanExecution? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    ExecutionType,
    ExecutionRecordId,
    LinkedOn,
    LinkedBy,
    Notes
FROM InspectionTestPlanExecutions
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        public List<InspectionTestPlanExecution> GetByItem(
            Guid inspectionTestPlanItemId)
        {
            var list = new List<InspectionTestPlanExecution>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    ExecutionType,
    ExecutionRecordId,
    LinkedOn,
    LinkedBy,
    Notes
FROM InspectionTestPlanExecutions
WHERE InspectionTestPlanItemId = $itemId
ORDER BY LinkedOn, Id;";

            cmd.Parameters.AddWithValue(
                "$itemId",
                inspectionTestPlanItemId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        public List<InspectionTestPlanExecution> GetByExecutionRecord(
            string executionType,
            string executionRecordId)
        {
            var list = new List<InspectionTestPlanExecution>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanItemId,
    ExecutionType,
    ExecutionRecordId,
    LinkedOn,
    LinkedBy,
    Notes
FROM InspectionTestPlanExecutions
WHERE ExecutionType = $executionType
  AND ExecutionRecordId = $executionRecordId
ORDER BY LinkedOn, Id;";

            cmd.Parameters.AddWithValue(
                "$executionType",
                executionType);

            cmd.Parameters.AddWithValue(
                "$executionRecordId",
                executionRecordId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        public bool Exists(
            Guid inspectionTestPlanItemId,
            string executionType,
            string executionRecordId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT COUNT(1)
FROM InspectionTestPlanExecutions
WHERE InspectionTestPlanItemId = $itemId
  AND ExecutionType = $executionType
  AND ExecutionRecordId = $executionRecordId;";

            cmd.Parameters.AddWithValue(
                "$itemId",
                inspectionTestPlanItemId.ToString());

            cmd.Parameters.AddWithValue(
                "$executionType",
                executionType);

            cmd.Parameters.AddWithValue(
                "$executionRecordId",
                executionRecordId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
DELETE FROM InspectionTestPlanExecutions
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        private static void AddParameters(
            SqliteCommand cmd,
            InspectionTestPlanExecution item)
        {
            cmd.Parameters.AddWithValue(
                "$id",
                item.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$itemId",
                item.InspectionTestPlanItemId.ToString());

            cmd.Parameters.AddWithValue(
                "$executionType",
                item.ExecutionType);

            cmd.Parameters.AddWithValue(
                "$executionRecordId",
                item.ExecutionRecordId);

            cmd.Parameters.AddWithValue(
                "$linkedOn",
                item.LinkedOn.ToString("O"));

            cmd.Parameters.AddWithValue(
                "$linkedBy",
                (object?)item.LinkedBy ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$notes",
                (object?)item.Notes ?? DBNull.Value);
        }

        private static InspectionTestPlanExecution Map(
            SqliteDataReader reader)
        {
            return new InspectionTestPlanExecution
            {
                Id = Guid.Parse(reader.GetString(0)),

                InspectionTestPlanItemId =
                    Guid.Parse(reader.GetString(1)),

                ExecutionType =
                    reader.GetString(2),

                ExecutionRecordId =
                    reader.GetString(3),

                LinkedOn =
                    DateTime.Parse(reader.GetString(4)),

                LinkedBy =
                    reader.IsDBNull(5)
                        ? null
                        : reader.GetString(5),

                Notes =
                    reader.IsDBNull(6)
                        ? null
                        : reader.GetString(6)
            };
        }
    }
}

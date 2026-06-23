using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Data.Repositories
{
    public class WeldNdtRepository
    {
        private readonly string _connectionString;

        public WeldNdtRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<WeldNdtResult> GetByWeld(Guid weldId)
        {
            var list = new List<WeldNdtResult>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM WeldNdtResults
WHERE WeldId = $weldId
ORDER BY InspectionDate DESC;";

            cmd.Parameters.AddWithValue(
                "$weldId",
                weldId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new WeldNdtResult
                {
                    Id = Guid.Parse(reader["Id"]?.ToString() ?? Guid.Empty.ToString()),

                    WeldId = Guid.Parse(
                        reader["WeldId"]?.ToString() ?? Guid.Empty.ToString()),

                    NdtMethod =
Enum.TryParse<NdtMethodType>(
reader["NdtMethod"]?.ToString(),
out var method)
? method
: NdtMethodType.RT,

                    Result =
Enum.TryParse<NdtResultType>(
reader["Result"]?.ToString(),
out var result)
? result
: NdtResultType.Pending,


                    ReportNumber =
                        reader["ReportNumber"]?.ToString() ?? "",

                    InspectionDate =
                        DateTime.Parse(
                            reader["InspectionDate"]?.ToString() ?? DateTime.MinValue.ToString()),

                    InspectorName =
                        reader["InspectorName"]?.ToString() ?? "",

                    AcceptanceCriteria =
                        reader["AcceptanceCriteria"]?.ToString() ?? "",

                    Remarks =
                        reader["Remarks"]?.ToString() ?? "",

                    RequiresRepair =
                        Convert.ToBoolean(
                            reader["RequiresRepair"]),

                    RepairCycle =
                        Convert.ToInt32(
                            reader["RepairCycle"]),

                    IsReinspection =
                        Convert.ToBoolean(
                            reader["IsReinspection"])
                });
            }

            return list;
        }

public List<WeldNdtResult> GetAll()
        {
            var list =
                new List<WeldNdtResult>();

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText =
                "SELECT * FROM WeldNdtResults";

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(
                    new WeldNdtResult
                    {
                        Id =
                            Guid.Parse(
                                reader["Id"].ToString()!),

                        WeldId =
                            Guid.Parse(
                                reader["WeldId"].ToString()!),

                        Result =
                            Enum.Parse<NdtResultType>(
                                reader["Result"]
                                    .ToString()!)
                    });
            }

            return list;
        }


        public void Add(WeldNdtResult ndt)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO WeldNdtResults
(
    Id,
    WeldId,
    NdtMethod,
    Result,
    ReportNumber,
    InspectionDate,
    InspectorName,
    AcceptanceCriteria,
    Remarks,
    RequiresRepair,
    RepairCycle,
    IsReinspection
)
VALUES
(
    $id,
    $weldId,
    $method,
    $result,
    $report,
    $date,
    $inspector,
    $criteria,
    $remarks,
    $repair,
    $cycle,
    $reinspection
);";

            cmd.Parameters.AddWithValue(
                "$id",
                ndt.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$weldId",
                ndt.WeldId.ToString());

            cmd.Parameters.AddWithValue(
                "$method",
                ndt.NdtMethod.ToString());

            cmd.Parameters.AddWithValue(
                "$result",
                ndt.Result.ToString());

            cmd.Parameters.AddWithValue(
                "$report",
                ndt.ReportNumber);

            cmd.Parameters.AddWithValue(
                "$date",
                ndt.InspectionDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue(
                "$inspector",
                ndt.InspectorName);

            cmd.Parameters.AddWithValue(
                "$criteria",
                ndt.AcceptanceCriteria);

            cmd.Parameters.AddWithValue(
                "$remarks",
                ndt.Remarks);

            cmd.Parameters.AddWithValue(
                "$repair",
                ndt.RequiresRepair);

            cmd.Parameters.AddWithValue(
                "$cycle",
                ndt.RepairCycle);

            cmd.Parameters.AddWithValue(
                "$reinspection",
                ndt.IsReinspection);

            cmd.ExecuteNonQuery();
        }
    }
}
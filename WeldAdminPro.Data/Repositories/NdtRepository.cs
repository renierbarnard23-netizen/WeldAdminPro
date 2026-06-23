using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class NdtRepository
    {
        private readonly string _connectionString;

        public NdtRepository(
        string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public List<WeldNdtResult>
            GetByWeld(Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"

SELECT *
FROM WeldNdtResults
WHERE WeldId = $WeldId
ORDER BY InspectionDate";

        command.Parameters.AddWithValue(
            "$WeldId",
            weldId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadResults(reader);
        }

        public List<WeldNdtResult>
            GetByProject(
            List<Guid> weldIds)
        {
            var results =
                new List<WeldNdtResult>();

            foreach (var weldId
                in weldIds)
            {
                results.AddRange(
                    GetByWeld(weldId));
            }

            return results;
        }

        private List<WeldNdtResult>
            ReadResults(
            SqliteDataReader reader)
        {
            var results =
                new List<WeldNdtResult>();

            while (reader.Read())
            {
                results.Add(
                    new WeldNdtResult
                    {
                        Id =
                            Guid.Parse(
                                reader["Id"]
                                    .ToString()!),

                        WeldId =
                            Guid.Parse(
                                reader["WeldId"]
                                    .ToString()!),

                        NdtMethod =
                            (NdtMethodType)
                            Convert.ToInt32(
                                reader["NdtMethod"]),

                        Result =
                            (NdtResultType)
                            Convert.ToInt32(
                                reader["Result"]),

                        InspectionDate =
                            DateTime.Parse(
                                reader["InspectionDate"]
                                    .ToString()!),

                        InspectorName =
                            reader["InspectorName"]
                                ?.ToString() ?? "",

                        ReportNumber =
                            reader["ReportNumber"]
                                ?.ToString() ?? "",

                        AcceptanceCriteria =
                            reader["AcceptanceCriteria"]
                                ?.ToString() ?? "",

                        Remarks =
                            reader["Remarks"]
                                ?.ToString() ?? "",

                        RequiresRepair =
                            Convert.ToInt32(
                                reader["RequiresRepair"]) == 1,

                        RepairCycle =
                            Convert.ToInt32(
                                reader["RepairCycle"]),

                        IsReinspection =
                            Convert.ToInt32(
                                reader["IsReinspection"]) == 1
                    });
            }

            return results;
        }
    }
}

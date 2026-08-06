using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class RepairRepository
    {
        private readonly string _connectionString;

        public RepairRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            RepairRecord repair)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
INSERT INTO RepairRecords
(
    Id,
    WeldId,
    NcrId,
    RepairNumber,
    Reason,
    AuthorizedBy,
    RequestedDate,
    AuthorizedDate,
    ExcavationMethod,
    RepairWpsNumber,
    RepairedByWelder,
    ReinspectionResult,
    Notes,
    Status,
    CompletedDate
)
VALUES
(
    $Id,
    $WeldId,
    $NcrId,
    $RepairNumber,
    $Reason,
    $AuthorizedBy,
    $RequestedDate,
    $AuthorizedDate,
    $ExcavationMethod,
    $RepairWpsNumber,
    $RepairedByWelder,
    $ReinspectionResult,
    $Notes,
    $Status,
    $CompletedDate
)";

            command.Parameters.AddWithValue(
                "$Id",
                repair.Id.ToString());

            command.Parameters.AddWithValue(
                "$WeldId",
                repair.WeldId.ToString());

            command.Parameters.AddWithValue(
                "$NcrId",
                repair.NcrId.HasValue
                    ? repair.NcrId.Value.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "$RepairNumber",
                repair.RepairNumber);

            command.Parameters.AddWithValue(
                "$Reason",
                repair.Reason);

            command.Parameters.AddWithValue(
                "$AuthorizedBy",
                repair.AuthorizedBy ?? "");

            command.Parameters.AddWithValue(
                "$RequestedDate",
                repair.RequestedDate.ToString("O"));

            command.Parameters.AddWithValue(
                "$AuthorizedDate",
                repair.AuthorizedDate?.ToString("O")
                ?? "");

            command.Parameters.AddWithValue(
                "$ExcavationMethod",
                repair.ExcavationMethod ?? "");

            command.Parameters.AddWithValue(
                "$RepairWpsNumber",
                repair.RepairWpsNumber ?? "");

            command.Parameters.AddWithValue(
                "$RepairedByWelder",
                repair.RepairedByWelder ?? "");

            command.Parameters.AddWithValue(
                "$ReinspectionResult",
                repair.ReinspectionResult ?? "");

            command.Parameters.AddWithValue(
                "$Notes",
                repair.Notes ?? "");

            command.Parameters.AddWithValue(
                "$Status",
                (int)repair.Status);

            command.Parameters.AddWithValue(
                "$CompletedDate",
                repair.CompletedDate?.ToString("O")
                ?? "");

            command.ExecuteNonQuery();
        }

        public List<RepairRecord> GetByWeld(
            Guid weldId)
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
FROM RepairRecords
WHERE WeldId = $WeldId
ORDER BY RepairNumber";

            command.Parameters.AddWithValue(
                "$WeldId",
                weldId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadRepairs(reader);
        }
        public List<RepairRecord> GetByNcr(
            Guid ncrId)
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
FROM RepairRecords
WHERE NcrId = $NcrId
ORDER BY RepairNumber;
";

            command.Parameters.AddWithValue(
                "$NcrId",
                ncrId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadRepairs(reader);
        }

public List<RepairRecord> GetAll()
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
FROM RepairRecords
ORDER BY RequestedDate DESC";

            using var reader =
                command.ExecuteReader();

            return ReadRepairs(reader);
        }

        public void Update(
            RepairRecord repair)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
UPDATE RepairRecords
SET
    
    NcrId = $NcrId,
    Reason = $Reason,
    AuthorizedBy = $AuthorizedBy,
    RequestedDate = $RequestedDate,
    AuthorizedDate = $AuthorizedDate,
    ExcavationMethod = $ExcavationMethod,
    RepairWpsNumber = $RepairWpsNumber,
    RepairedByWelder = $RepairedByWelder,
    ReinspectionResult = $ReinspectionResult,
    Notes = $Notes,
    Status = $Status,
    CompletedDate = $CompletedDate
WHERE Id = $Id";

            command.Parameters.AddWithValue(
                "$Id",
                repair.Id.ToString());

            command.Parameters.AddWithValue(
                "$NcrId",
                repair.NcrId.HasValue
                    ? repair.NcrId.Value.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "$Reason",
                repair.Reason);

            command.Parameters.AddWithValue(
                "$AuthorizedBy",
                repair.AuthorizedBy ?? "");

            command.Parameters.AddWithValue(
                "$RequestedDate",
                repair.RequestedDate.ToString("O"));

            command.Parameters.AddWithValue(
                "$AuthorizedDate",
                repair.AuthorizedDate?.ToString("O")
                ?? "");

            command.Parameters.AddWithValue(
                "$ExcavationMethod",
                repair.ExcavationMethod ?? "");

            command.Parameters.AddWithValue(
                "$RepairWpsNumber",
                repair.RepairWpsNumber ?? "");

            command.Parameters.AddWithValue(
                "$RepairedByWelder",
                repair.RepairedByWelder ?? "");

            command.Parameters.AddWithValue(
                "$ReinspectionResult",
                repair.ReinspectionResult ?? "");

            command.Parameters.AddWithValue(
                "$Notes",
                repair.Notes ?? "");

            command.Parameters.AddWithValue(
                "$Status",
                (int)repair.Status);

            command.Parameters.AddWithValue(
                "$CompletedDate",
                repair.CompletedDate?.ToString("O")
                ?? "");

            command.ExecuteNonQuery();
        }

        private List<RepairRecord> ReadRepairs(
            SqliteDataReader reader)
        {
            var repairs =
                new List<RepairRecord>();

            while (reader.Read())
            {
                repairs.Add(
                    new RepairRecord
                    {
                        Id =
                            Guid.Parse(
                                reader["Id"]
                                    .ToString()!),

                        WeldId =
                            Guid.Parse(
                                reader["WeldId"]
                                    .ToString()!),

                        NcrId =
                            string.IsNullOrWhiteSpace(
                                reader["NcrId"]?.ToString())
                                ? null
                                : Guid.Parse(
                                    reader["NcrId"]
                                        .ToString()!),

                        RepairNumber =
                            Convert.ToInt32(
                                reader["RepairNumber"]),

                        Reason =
                            reader["Reason"]
                                ?.ToString() ?? "",

                        AuthorizedBy =
                            reader["AuthorizedBy"]
                                ?.ToString() ?? "",

                        RequestedDate =
                            DateTime.Parse(
                                reader["RequestedDate"]
                                    .ToString()!),

                        AuthorizedDate =
                            string.IsNullOrWhiteSpace(
                                reader["AuthorizedDate"]
                                    ?.ToString())
                            ? null
                            : DateTime.Parse(
                                reader["AuthorizedDate"]
                                    .ToString()!),

                        ExcavationMethod =
                            reader["ExcavationMethod"]
                                ?.ToString() ?? "",

                        RepairWpsNumber =
                            reader["RepairWpsNumber"]
                                ?.ToString() ?? "",

                        RepairedByWelder =
                            reader["RepairedByWelder"]
                                ?.ToString() ?? "",

                        ReinspectionResult =
                            reader["ReinspectionResult"]
                                ?.ToString() ?? "",

                        Notes =
                            reader["Notes"]
                                ?.ToString() ?? "",

                        Status =
                            (RepairStatus)
                            Convert.ToInt32(
                                reader["Status"]),

                        CompletedDate =
                            string.IsNullOrWhiteSpace(
                            reader["CompletedDate"]
                                ?.ToString())
                                ? null
                                : DateTime.Parse(
                            reader["CompletedDate"]
                                .ToString()!),
                    });
            }

            return repairs;
        }
    }
}




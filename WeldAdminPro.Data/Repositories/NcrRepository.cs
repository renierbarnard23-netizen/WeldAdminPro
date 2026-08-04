using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class NcrRepository
    {
        private readonly string _connectionString;

        public NcrRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"INSERT INTO NcrRecords
                (
                    Id,
                    WeldId,
                    WeldNumber,
                    Description,
                    NcrNumber,
                    RootCause,
                    CorrectiveAction,
                    PreventiveAction,
                    RaisedBy,
                    RaisedDate,
                    AssignedTo,
                    DueDate,
                    Status,
                    IsClosed,
                    ClosedBy,
                    ClosedDate,
                    DispositionType,
                    DispositionApprovedBy,
                    DispositionApprovedDate,
                    VerificationBy,
                    VerificationDate,
                    RequiresCustomerApproval,
                    CustomerApproved,
                    CustomerApprovalReference
                )
                VALUES
                (
                    @Id,
                    @WeldId,
                    @WeldNumber,
                    @Description,
                    @NcrNumber,
                    @RootCause,
                    @CorrectiveAction,
                    @PreventiveAction,
                    @RaisedBy,
                    @RaisedDate,
                    @AssignedTo,
                    @DueDate,
                    @Status,
                    @IsClosed,
                    @ClosedBy,
                    @ClosedDate,
                    @DispositionType,
                    @DispositionApprovedBy,
                    @DispositionApprovedDate,
                    @VerificationBy,
                    @VerificationDate,
                    @RequiresCustomerApproval,
                    @CustomerApproved,
                    @CustomerApprovalReference
                )",
                ToParameters(item));
        }

        public List<NcrRecord> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    @"SELECT *
                      FROM NcrRecords
                      ORDER BY RaisedDate DESC");

            return rows
                .Select(Map)
                .ToList();
        }

        public List<NcrRecord> GetByWeld(
            Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    @"SELECT *
                      FROM NcrRecords
                      WHERE WeldId = @WeldId
                      ORDER BY RaisedDate DESC",
                    new
                    {
                        WeldId =
                            weldId.ToString()
                    });

            return rows
                .Select(Map)
                .ToList();
        }

        public void Update(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET
                    WeldNumber = @WeldNumber,
                    Description = @Description,
                    NcrNumber = @NcrNumber,
                    RootCause = @RootCause,
                    CorrectiveAction = @CorrectiveAction,
                    PreventiveAction = @PreventiveAction,
                    RaisedBy = @RaisedBy,
                    RaisedDate = @RaisedDate,
                    AssignedTo = @AssignedTo,
                    DueDate = @DueDate,
                    Status = @Status,
                    IsClosed = @IsClosed,
                    ClosedBy = @ClosedBy,
                    ClosedDate = @ClosedDate,
                    DispositionType = @DispositionType,
                    DispositionApprovedBy = @DispositionApprovedBy,
                    DispositionApprovedDate = @DispositionApprovedDate,
                    VerificationBy = @VerificationBy,
                    VerificationDate = @VerificationDate,
                    RequiresCustomerApproval = @RequiresCustomerApproval,
                    CustomerApproved = @CustomerApproved,
                    CustomerApprovalReference = @CustomerApprovalReference
                  WHERE Id = @Id",
                ToParameters(item));
        }

        private static object ToParameters(
            NcrRecord item)
        {
            return new
            {
                Id =
                    item.Id.ToString(),

                WeldId =
                    item.WeldId.ToString(),

                item.WeldNumber,
                item.Description,
                item.NcrNumber,
                item.RootCause,
                item.CorrectiveAction,
                item.PreventiveAction,
                item.RaisedBy,
                item.RaisedDate,
                item.AssignedTo,
                item.DueDate,

                Status =
                    (int)item.Status,

                IsClosed =
                    item.IsClosed ? 1 : 0,

                item.ClosedBy,
                item.ClosedDate,

                DispositionType =
                    item.DispositionType.HasValue
                        ? (int)item.DispositionType.Value
                        : (int?)null,

                item.DispositionApprovedBy,
                item.DispositionApprovedDate,
                item.VerificationBy,
                item.VerificationDate,

                RequiresCustomerApproval =
                    item.RequiresCustomerApproval ? 1 : 0,

                CustomerApproved =
                    item.CustomerApproved ? 1 : 0,

                item.CustomerApprovalReference
            };
        }

        public string GetNextNcrNumber()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
SELECT NcrNumber
FROM NcrRecords
WHERE NcrNumber IS NOT NULL
  AND NcrNumber <> '';";

            using var reader =
                cmd.ExecuteReader();

            var maxNumber = 0;

            while (reader.Read())
            {
                var ncrNumber =
                    reader["NcrNumber"]
                        ?.ToString();

                if (string.IsNullOrWhiteSpace(
                    ncrNumber))
                {
                    continue;
                }

                if (!ncrNumber.StartsWith(
                    "NCR-",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var numberPart =
                    ncrNumber.Substring(4);

                if (int.TryParse(
                    numberPart,
                    out var number)
                    &&
                    number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"NCR-{(maxNumber + 1):000}";
        }

        private static NcrRecord Map(
            dynamic row)
        {
            return new NcrRecord
            {
                Id =
                    Guid.Parse(
                        (string)row.Id),

                WeldId =
                    Guid.Parse(
                        (string)row.WeldId),

                WeldNumber =
                    row.WeldNumber?.ToString()
                    ?? string.Empty,

                NcrNumber =
                    row.NcrNumber?.ToString()
                    ?? string.Empty,

                Description =
                    row.Description?.ToString()
                    ?? string.Empty,

                RootCause =
                    row.RootCause?.ToString()
                    ?? string.Empty,

                CorrectiveAction =
                    row.CorrectiveAction?.ToString()
                    ?? string.Empty,

                PreventiveAction =
                    row.PreventiveAction?.ToString()
                    ?? string.Empty,

                RaisedBy =
                    row.RaisedBy?.ToString()
                    ?? string.Empty,

                RaisedDate =
                    Convert.ToDateTime(
                        row.RaisedDate),

                AssignedTo =
                    row.AssignedTo?.ToString()
                    ?? string.Empty,

                DueDate =
                    row.DueDate == null
                        ? null
                        : Convert.ToDateTime(
                            row.DueDate),

                Status =
                    (NcrStatus)
                    Convert.ToInt32(
                        row.Status),

                IsClosed =
                    Convert.ToBoolean(
                        row.IsClosed),

                ClosedBy =
                    row.ClosedBy?.ToString()
                    ?? string.Empty,

                ClosedDate =
                    row.ClosedDate == null
                        ? null
                        : Convert.ToDateTime(
                            row.ClosedDate),

                DispositionType =
                    row.DispositionType == null
                        ? null
                        : (NcrDispositionType?)
                            Convert.ToInt32(
                                row.DispositionType),

                DispositionApprovedBy =
                    row.DispositionApprovedBy?.ToString(),

                DispositionApprovedDate =
                    row.DispositionApprovedDate == null
                        ? null
                        : Convert.ToDateTime(
                            row.DispositionApprovedDate),

                VerificationBy =
                    row.VerificationBy?.ToString(),

                VerificationDate =
                    row.VerificationDate == null
                        ? null
                        : Convert.ToDateTime(
                            row.VerificationDate),

                RequiresCustomerApproval =
                    row.RequiresCustomerApproval != null
                    &&
                    Convert.ToBoolean(
                        row.RequiresCustomerApproval),

                CustomerApproved =
                    row.CustomerApproved != null
                    &&
                    Convert.ToBoolean(
                        row.CustomerApproved),

                CustomerApprovalReference =
                    row.CustomerApprovalReference?.ToString()
            };
        }
    }
}

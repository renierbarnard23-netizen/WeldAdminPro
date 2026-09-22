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

            connection.Open();

            using var transaction =
                connection.BeginTransaction(deferred: false);

            try
            {
                var maxNumber =
                    connection.ExecuteScalar<int?>(
                        @"
                        SELECT MAX(
                            CAST(
                                SUBSTR(NcrNumber, 5)
                                AS INTEGER))
                        FROM NcrRecords
                        WHERE NcrNumber IS NOT NULL
                          AND TRIM(NcrNumber) <> ''
                          AND UPPER(NcrNumber) LIKE 'NCR-%'
                          AND SUBSTR(NcrNumber, 5) GLOB '[0-9]*';",
                        transaction: transaction)
                    ?? 0;

                item.NcrNumber =
                    $"NCR-{(maxNumber + 1):000}";

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
                        CustomerApprovalReference,
                        Category,
                        CustomReason,
                        IsWeldingRelated
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
                        @CustomerApprovalReference,
                        @Category,
                        @CustomReason,
                        @IsWeldingRelated
                    )",
                    ToParameters(item),
                    transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        public NcrRecord? GetById(
            Guid id)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var row =
                connection.QueryFirstOrDefault(
                    @"SELECT *
                      FROM NcrRecords
                      WHERE Id = @Id",
                    new
                    {
                        Id = id.ToString()
                    });

            return row == null
                ? null
                : Map(row);
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

        /// <summary>
        /// Updates only ordinary NCR information. Lifecycle-controlled fields
        /// are intentionally excluded and must be changed through dedicated
        /// application-service operations.
        /// </summary>
        public void UpdateEditable(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET
                    Description = @Description,
                    RootCause = @RootCause,
                    CorrectiveAction = @CorrectiveAction,
                    PreventiveAction = @PreventiveAction,
                    AssignedTo = @AssignedTo,
                    DueDate = @DueDate,
                    Category = @Category,
                    CustomReason = @CustomReason,
                    IsWeldingRelated = @IsWeldingRelated
                  WHERE Id = @Id",
                ToEditableParameters(item));
        }

        /// <summary>
        /// Persists a workflow status change after the application service has
        /// validated the transition.
        /// </summary>
        public void UpdateStatus(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET Status = @Status
                  WHERE Id = @Id",
                new
                {
                    Id = item.Id.ToString(),
                    Status = (int)item.Status
                });
        }

        /// <summary>
        /// Persists disposition and customer-approval data through the
        /// controlled disposition operation.
        /// </summary>
        public void UpdateDisposition(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET
                    DispositionType = @DispositionType,
                    DispositionApprovedBy = @DispositionApprovedBy,
                    DispositionApprovedDate = @DispositionApprovedDate,
                    RequiresCustomerApproval = @RequiresCustomerApproval,
                    CustomerApproved = @CustomerApproved,
                    CustomerApprovalReference = @CustomerApprovalReference
                  WHERE Id = @Id",
                new
                {
                    Id = item.Id.ToString(),
                    DispositionType = item.DispositionType.HasValue
                        ? (int)item.DispositionType.Value
                        : (int?)null,
                    item.DispositionApprovedBy,
                    item.DispositionApprovedDate,
                    RequiresCustomerApproval = item.RequiresCustomerApproval ? 1 : 0,
                    CustomerApproved = item.CustomerApproved ? 1 : 0,
                    item.CustomerApprovalReference
                });
        }

        /// <summary>
        /// Persists final verification data through the controlled verification
        /// operation.
        /// </summary>
        public void UpdateVerification(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET
                    VerificationBy = @VerificationBy,
                    VerificationDate = @VerificationDate
                  WHERE Id = @Id",
                new
                {
                    Id = item.Id.ToString(),
                    item.VerificationBy,
                    item.VerificationDate
                });
        }

        /// <summary>
        /// Persists closure data through the controlled closure operation.
        /// </summary>
        public void UpdateClosure(
            NcrRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE NcrRecords
                  SET
                    Status = @Status,
                    IsClosed = @IsClosed,
                    ClosedBy = @ClosedBy,
                    ClosedDate = @ClosedDate
                  WHERE Id = @Id",
                new
                {
                    Id = item.Id.ToString(),
                    Status = (int)item.Status,
                    IsClosed = item.IsClosed ? 1 : 0,
                    item.ClosedBy,
                    item.ClosedDate
                });
        }

        private static object ToEditableParameters(
            NcrRecord item)
        {
            return new
            {
                Id = item.Id.ToString(),
                item.Description,
                item.RootCause,
                item.CorrectiveAction,
                item.PreventiveAction,
                item.AssignedTo,
                item.DueDate,
                item.Category,
                item.CustomReason,
                IsWeldingRelated = item.IsWeldingRelated ? 1 : 0
            };
        }

        private static object ToParameters(
            NcrRecord item)
        {
            return new
            {
                Id =
                    item.Id.ToString(),

                WeldId =
                    item.WeldId.HasValue
                        ? item.WeldId.Value.ToString()
                        : null,

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

                item.CustomerApprovalReference,

                item.Category,
                item.CustomReason,

                IsWeldingRelated =
                    item.IsWeldingRelated ? 1 : 0
            };
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
                    row.WeldId == null
                    || row.WeldId is DBNull
                    || string.IsNullOrWhiteSpace(
                        row.WeldId.ToString())
                        ? null
                        : Guid.Parse(
                            row.WeldId.ToString()),

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
                    row.CustomerApprovalReference?.ToString(),

                Category =
                    row.Category?.ToString()
                    ?? string.Empty,

                CustomReason =
                    row.CustomReason?.ToString()
                    ?? string.Empty,

                IsWeldingRelated =
                    row.IsWeldingRelated != null
                    &&
                    Convert.ToBoolean(
                        row.IsWeldingRelated)
            };
        }
    }
}



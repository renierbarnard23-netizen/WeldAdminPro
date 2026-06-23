using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class CapaRepository
    {
        private readonly string _connectionString;

        public CapaRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        // =====================================
        // ADD
        // =====================================

        public void Add(
            CapaRecord record)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"
INSERT INTO CapaRecords
(
    Id,
    NcrId,
    CapaNumber,
    Title,
    RootCause,
    CorrectiveAction,
    PreventiveAction,
    AssignedTo,
    DueDate,
    CreatedDate,
    CompletedDate,
    CreatedBy,
    VerifiedBy,
    VerifiedDate,
    IsEffective,
    Priority,
    Status
)
VALUES
(
    @Id,
    @NcrId,
    @CapaNumber,
    @Title,
    @RootCause,
    @CorrectiveAction,
    @PreventiveAction,
    @AssignedTo,
    @DueDate,
    @CreatedDate,
    @CompletedDate,
    @CreatedBy,
    @VerifiedBy,
    @VerifiedDate,
    @IsEffective,
    @Priority,
    @Status
)",
                new
                {
                    Id =
                        record.Id.ToString(),

                    NcrId =
                        record.NcrId.ToString(),

                    record.CapaNumber,

                    record.Title,

                    record.RootCause,

                    record.CorrectiveAction,

                    record.PreventiveAction,

                    record.AssignedTo,

                    DueDate =
                        record.DueDate.ToString("O"),

                    CreatedDate =
                        record.CreatedDate.ToString("O"),

                    CompletedDate =
                        record.CompletedDate?
                            .ToString("O"),

                    record.CreatedBy,

                    record.VerifiedBy,

                    VerifiedDate =
                        record.VerifiedDate?
                            .ToString("O"),

                    IsEffective =
                        record.IsEffective ? 1 : 0,

                    Priority =
                        (int)record.Priority,

                    Status =
                        (int)record.Status
                });
        }

        // =====================================
        // UPDATE
        // =====================================

        public void Update(
            CapaRecord record)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"
UPDATE CapaRecords
SET
    Title = @Title,
    RootCause = @RootCause,
    CorrectiveAction = @CorrectiveAction,
    PreventiveAction = @PreventiveAction,
    AssignedTo = @AssignedTo,
    DueDate = @DueDate,
    CompletedDate = @CompletedDate,
    VerifiedBy = @VerifiedBy,
    VerifiedDate = @VerifiedDate,
    IsEffective = @IsEffective,
    Priority = @Priority,
    Status = @Status
WHERE Id = @Id",
                new
                {
                    Id =
                        record.Id.ToString(),

                    record.Title,

                    record.RootCause,

                    record.CorrectiveAction,

                    record.PreventiveAction,

                    record.AssignedTo,

                    DueDate =
                        record.DueDate.ToString("O"),

                    CompletedDate =
                        record.CompletedDate?
                            .ToString("O"),

                    record.VerifiedBy,

                    VerifiedDate =
                        record.VerifiedDate?
                            .ToString("O"),

                    IsEffective =
                        record.IsEffective ? 1 : 0,

                    Priority =
                        (int)record.Priority,

                    Status =
                        (int)record.Status
                });
        }

        // =====================================
        // GET ALL
        // =====================================

        public List<CapaRecord> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows =
                connection.Query(
                    "SELECT * FROM CapaRecords");

            return rows.Select(row =>
                new CapaRecord
                {
                    Id =
                        Guid.Parse(row.Id),

                    NcrId =
                        Guid.Parse(row.NcrId),

                    CapaNumber =
                        row.CapaNumber,

                    Title =
                        row.Title,

                    RootCause =
                        row.RootCause,

                    CorrectiveAction =
                        row.CorrectiveAction,

                    PreventiveAction =
                        row.PreventiveAction,

                    AssignedTo =
                        row.AssignedTo,

                    DueDate =
                        DateTime.Parse(
                            row.DueDate.ToString()),

                    CreatedDate =
                        DateTime.Parse(
                            row.CreatedDate.ToString()),

                    CompletedDate =
                        row.CompletedDate == null
                            ? null
                            : DateTime.Parse(
                                row.CompletedDate.ToString()),

                    CreatedBy =
                        row.CreatedBy,

                    VerifiedBy =
                        row.VerifiedBy,

                    VerifiedDate =
                        row.VerifiedDate == null
                            ? null
                            : DateTime.Parse(
                                row.VerifiedDate.ToString()),

                    IsEffective =
                        Convert.ToInt32(
                            row.IsEffective) == 1,

                    Priority =
                        (Core.Quality.Enums.CapaPriority)
                        Convert.ToInt32(
                            row.Priority),

                    Status =
                        (Core.Quality.Enums.CapaStatus)
                        Convert.ToInt32(
                            row.Status)
                })
                .ToList();
        }
    }
}

using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
                    ClosedDate
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
                    @ClosedDate
                )",
                new
                {
                    Id =
                        item.Id.ToString(),

                    WeldId =
                        item.WeldId.ToString(),

                    item.WeldNumber,

                    item.NcrNumber,

                    item.Description,

                    item.RootCause,

                    item.CorrectiveAction,

                    item.PreventiveAction,

                    item.RaisedBy,

                    RaisedDate =
                    item.RaisedDate,

                    item.AssignedTo,

                    item.DueDate,

                    Status =
                        (int)item.Status,

                    IsClosed =
                        item.IsClosed ? 1 : 0,

                    item.ClosedBy,

                    item.ClosedDate
                });
        }

        public List<NcrRecord> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM NcrRecords");

            var result =
                new List<NcrRecord>();

            foreach (var row in rows)
            {
                result.Add(
                    new NcrRecord
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        WeldId =
                            Guid.Parse(
                                (string)row.WeldId),

                        WeldNumber =
                            row.WeldNumber?.ToString()
                            ?? "",

                        Description =
                            row.Description?.ToString()
                            ?? "",

                        NcrNumber =
    row.NcrNumber?.ToString()
    ?? "",

                        RootCause =
                            row.RootCause?.ToString()
                            ?? "",

                        CorrectiveAction =
                            row.CorrectiveAction?.ToString()
                            ?? "",

                        PreventiveAction =
                            row.PreventiveAction?.ToString()
                            ?? "",

                        RaisedBy =
                            row.RaisedBy?.ToString()
                            ?? "",

                        RaisedDate =
                            Convert.ToDateTime(
                                row.RaisedDate),

                        AssignedTo =
                            row.AssignedTo?.ToString()
                            ?? "",

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
                            ?? "",

                        ClosedDate =
                            row.ClosedDate == null
                                ? null
                                : Convert.ToDateTime(
                                    row.ClosedDate)
                    });
            }

            return result;
        }

        public List<NcrRecord> GetByWeld(
            Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM NcrRecords
                  WHERE WeldId = @WeldId",
                new
                {
                    WeldId =
                        weldId.ToString()
                });

            var result =
                new List<NcrRecord>();

            foreach (var row in rows)
            {
                result.Add(
                    new NcrRecord
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        WeldId =
                            Guid.Parse(
                                (string)row.WeldId),

                        WeldNumber =
                            row.WeldNumber?.ToString()
                            ?? "",

                        Description =
                            row.Description?.ToString()
                            ?? "",

                        RootCause =
                            row.RootCause?.ToString()
                            ?? "",

                        CorrectiveAction =
                            row.CorrectiveAction?.ToString()
                            ?? "",

                        PreventiveAction =
                            row.PreventiveAction?.ToString()
                            ?? "",

                        RaisedBy =
                            row.RaisedBy?.ToString()
                            ?? "",

                        RaisedDate =
                            Convert.ToDateTime(
                                row.RaisedDate),

                        AssignedTo =
                            row.AssignedTo?.ToString()
                            ?? "",

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
                            ?? "",

                        ClosedDate =
                            row.ClosedDate == null
                                ? null
                                : Convert.ToDateTime(
                                    row.ClosedDate)
                    });
            }

            return result;
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
                    Description = @Description,
                    RootCause = @RootCause,
                    CorrectiveAction = @CorrectiveAction,
                    PreventiveAction = @PreventiveAction,
                    AssignedTo = @AssignedTo,
                    DueDate = @DueDate,
                    Status = @Status,
                    IsClosed = @IsClosed,
                    ClosedBy = @ClosedBy,
                    ClosedDate = @ClosedDate
                  WHERE Id = @Id",
                new
                {
                    item.Description,

                    item.RootCause,

                    item.CorrectiveAction,

                    item.PreventiveAction,

                    item.AssignedTo,

                    item.DueDate,

                    Status =
                        (int)item.Status,

                    IsClosed =
                        item.IsClosed ? 1 : 0,

                    item.ClosedBy,

                    item.ClosedDate,

                    Id =
                        item.Id.ToString()
                });
        }
    }
}
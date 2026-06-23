using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class HoldPointRepository
    {
        private readonly string _connectionString;

        public HoldPointRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(WeldHoldPoint item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);



            connection.Open();

            connection.Execute(
            @"INSERT INTO WeldHoldPoints
    (
        Id,
        WeldId,
        HoldPointType,
        Category,
        RequiredApproverRole,
        Status,
        IsMandatory,
        ApprovedBy,
        ApprovedDate,
        Comments
    )
    VALUES
    (
        @Id,
        @WeldId,
        @HoldPointType,
        @Category,
        @RequiredApproverRole,
        @Status,
        @IsMandatory,
        @ApprovedBy,
        @ApprovedDate,
        @Comments
    )",
            new
            {
                Id =
                    item.Id.ToString(),

                WeldId =
                    item.WeldId.ToString(),

                HoldPointType =
                    (int)item.HoldPointType,

                Category =
                    (int)item.Category,

                RequiredApproverRole =
                    (int)item.RequiredApproverRole,

                Status =
                    (int)item.Status,

                IsMandatory =
                    item.IsMandatory,

                ApprovedBy =
                    item.ApprovedBy,

                ApprovedDate =
                    item.ApprovedDate,

                Comments =
                    item.Comments
            });
        }

        public List<WeldHoldPoint> GetByWeld(
            Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
        @"SELECT *
      FROM WeldHoldPoints
      WHERE WeldId = @WeldId",
        new
        {
        WeldId = weldId.ToString()
        });

            var result = new List<WeldHoldPoint>();

            foreach (var row in rows)
            {
                result.Add(new WeldHoldPoint
                {
                    Id =
                        Guid.Parse((string)row.Id),

                    WeldId =
                        Guid.Parse((string)row.WeldId),

                    HoldPointType =
                        (Core.Quality.Enums.HoldPointType)
                        Convert.ToInt32(row.HoldPointType),

                    Category =
                        (Core.Quality.Enums.HoldPointCategory)
                        Convert.ToInt32(row.Category),

                    RequiredApproverRole =
                        (Core.Quality.Enums.HoldPointApproverRole)
                        Convert.ToInt32(
                        row.RequiredApproverRole),

                    Status =
                        (Core.Quality.Enums.HoldPointStatus)
                        Convert.ToInt32(row.Status),

                    IsMandatory =
                        Convert.ToBoolean(row.IsMandatory),

                    ApprovedBy =
                        row.ApprovedBy?.ToString() ?? "",

                    ApprovedDate =
                        row.ApprovedDate == null
                            ? null
                            : Convert.ToDateTime(row.ApprovedDate),

                    Comments =
                        row.Comments?.ToString() ?? ""
                });
            }

            return result;
        }

        public void Update(
    WeldHoldPoint item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE WeldHoldPoints
          SET
            Status = @Status,
            ApprovedBy = @ApprovedBy,
            ApprovedDate = @ApprovedDate,
            Comments = @Comments
          WHERE Id = @Id",
                new
                {
                    Status =
                        (int)item.Status,

                    ApprovedBy =
                        item.ApprovedBy,

                    ApprovedDate =
                        item.ApprovedDate,

                    Comments =
                        item.Comments,

                    Id =
                        item.Id.ToString()
                });
        }
    }
}
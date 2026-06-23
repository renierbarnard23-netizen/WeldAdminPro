using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class TurnoverPackageRepository
    {
        private readonly string _connectionString;

        public TurnoverPackageRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            TurnoverPackageRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"INSERT INTO TurnoverPackages
                (
                    Id,
                    ProjectId,
                    PackageNumber,
                    CreatedDate,
                    CreatedBy,
                    IsApproved,
                    ApprovedBy,
                    ApprovedDate
                )
                VALUES
                (
                    @Id,
                    @ProjectId,
                    @PackageNumber,
                    @CreatedDate,
                    @CreatedBy,
                    @IsApproved,
                    @ApprovedBy,
                    @ApprovedDate
                )",
                new
                {
                    Id =
                        item.Id.ToString(),

                    ProjectId =
                        item.ProjectId.ToString(),

                    item.PackageNumber,

                    item.CreatedDate,

                    item.CreatedBy,

                    IsApproved =
                        item.IsApproved ? 1 : 0,

                    item.ApprovedBy,

                    item.ApprovedDate
                });
        }

        public List<TurnoverPackageRecord>
            GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM TurnoverPackages");

            var result =
                new List<TurnoverPackageRecord>();

            foreach (var row in rows)
            {
                result.Add(
                    new TurnoverPackageRecord
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        ProjectId =
                            Guid.Parse(
                                (string)row.ProjectId),

                        PackageNumber =
                            row.PackageNumber
                                ?.ToString() ?? "",

                        CreatedDate =
                            Convert.ToDateTime(
                                row.CreatedDate),

                        CreatedBy =
                            row.CreatedBy
                                ?.ToString() ?? "",

                        IsApproved =
                            Convert.ToBoolean(
                                row.IsApproved),

                        ApprovedBy =
                            row.ApprovedBy
                                ?.ToString() ?? "",

                        ApprovedDate =
                            row.ApprovedDate == null
                                ? null
                                : Convert.ToDateTime(
                                    row.ApprovedDate)
                    });
            }

            return result;
        }

        public void Update(
            TurnoverPackageRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE TurnoverPackages
                  SET
                    IsApproved = @IsApproved,
                    ApprovedBy = @ApprovedBy,
                    ApprovedDate = @ApprovedDate
                  WHERE Id = @Id",
                new
                {
                    IsApproved =
                        item.IsApproved ? 1 : 0,

                    item.ApprovedBy,

                    item.ApprovedDate,

                    Id =
                        item.Id.ToString()
                });
        }
    }
}
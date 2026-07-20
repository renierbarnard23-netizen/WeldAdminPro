using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class WeldTraceabilityRepository
    {
        private readonly string _connectionString;

        public WeldTraceabilityRepository()
        {
            var appData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData);

            var dbFolder =
                Path.Combine(
                    appData,
                    "WeldAdminPro");

            Directory.CreateDirectory(
                dbFolder);

            _connectionString =
                $"Data Source={Path.Combine(
                    dbFolder,
                    "weldadmin.db")}";
        }

        public WeldTraceabilityRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            WeldTraceabilityRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"INSERT INTO WeldTraceability
                (
                    Id,
                    WeldId,
                    WpsNumber,
                    PqrNumber,
                    WelderQualification,
                    MaterialHeatNumber,
                    ConsumableBatch,
                    NdtReportNumber,
                    ReleaseCertificate
                )
                VALUES
                (
                    @Id,
                    @WeldId,
                    @WpsNumber,
                    @PqrNumber,
                    @WelderQualification,
                    @MaterialHeatNumber,
                    @ConsumableBatch,
                    @NdtReportNumber,
                    @ReleaseCertificate
                )",
                new
                {
                    Id =
                        item.Id.ToString(),

                    WeldId =
                        item.WeldId.ToString(),

                    item.WpsNumber,

                    item.PqrNumber,

                    item.WelderQualification,

                    item.MaterialHeatNumber,

                    item.ConsumableBatch,

                    item.NdtReportNumber,

                    item.ReleaseCertificate
                });
        }

        public void Update(
    WeldTraceabilityRecord item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE WeldTraceability
          SET
            WpsNumber = @WpsNumber,
            PqrNumber = @PqrNumber,
            WelderQualification = @WelderQualification,
            MaterialHeatNumber = @MaterialHeatNumber,
            ConsumableBatch = @ConsumableBatch,
            NdtReportNumber = @NdtReportNumber,
            ReleaseCertificate = @ReleaseCertificate
          WHERE Id = @Id",
                new
                {
                    item.WpsNumber,
                    item.PqrNumber,
                    item.WelderQualification,
                    item.MaterialHeatNumber,
                    item.ConsumableBatch,
                    item.NdtReportNumber,
                    item.ReleaseCertificate,

                    Id =
                        item.Id.ToString()
                });
        }

        public List<WeldTraceabilityRecord>
            GetByWeld(Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM WeldTraceability
                  WHERE WeldId = @WeldId",
                new
                {
                    WeldId =
                        weldId.ToString()
                });

            var result =
                new List<WeldTraceabilityRecord>();

            foreach (var row in rows)
            {
                result.Add(
                    new WeldTraceabilityRecord
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        WeldId =
                            Guid.Parse(
                                (string)row.WeldId),

                        WpsNumber =
                            row.WpsNumber?.ToString()
                            ?? "",

                        PqrNumber =
                            row.PqrNumber?.ToString()
                            ?? "",

                        WelderQualification =
                            row.WelderQualification?.ToString()
                            ?? "",

                        MaterialHeatNumber =
                            row.MaterialHeatNumber?.ToString()
                            ?? "",

                        ConsumableBatch =
                            row.ConsumableBatch?.ToString()
                            ?? "",

                        NdtReportNumber =
                            row.NdtReportNumber?.ToString()
                            ?? "",

                        ReleaseCertificate =
                            row.ReleaseCertificate?.ToString()
                            ?? ""
                    });
            }

            return result;
        }
    }
}
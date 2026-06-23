using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class QcpInspectionRuleRepository
    {
        private readonly string _connectionString;

        public QcpInspectionRuleRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            QcpInspectionRule item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"INSERT INTO QcpInspectionRules
                (
                    Id,
                    WeldType,
                    RequiredNdtType,
                    InspectionPercentage,
                    RequiresClientWitness,
                    RequiresHoldPoint
                )
                VALUES
                (
                    @Id,
                    @WeldType,
                    @RequiredNdtType,
                    @InspectionPercentage,
                    @RequiresClientWitness,
                    @RequiresHoldPoint
                )",
                new
                {
                    Id =
                        item.Id.ToString(),

                    item.WeldType,

                    RequiredNdtType =
                        (int)item.RequiredNdtType,

                    item.InspectionPercentage,

                    RequiresClientWitness =
                        item.RequiresClientWitness ? 1 : 0,

                    RequiresHoldPoint =
                        item.RequiresHoldPoint ? 1 : 0
                });
        }

        public List<QcpInspectionRule> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM QcpInspectionRules");

            var result =
                new List<QcpInspectionRule>();

            foreach (var row in rows)
            {
                result.Add(
                    new QcpInspectionRule
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        WeldType =
                            row.WeldType?.ToString()
                            ?? "",

                        RequiredNdtType =
                            (NdtType)
                            Convert.ToInt32(
                                row.RequiredNdtType),

                        InspectionPercentage =
                            Convert.ToDouble(
                                row.InspectionPercentage),

                        RequiresClientWitness =
                            Convert.ToBoolean(
                                row.RequiresClientWitness),

                        RequiresHoldPoint =
                            Convert.ToBoolean(
                                row.RequiresHoldPoint)
                    });
            }

            return result;
        }

        public void Update(
            QcpInspectionRule item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"UPDATE QcpInspectionRules
                  SET
                    WeldType = @WeldType,
                    RequiredNdtType = @RequiredNdtType,
                    InspectionPercentage = @InspectionPercentage,
                    RequiresClientWitness = @RequiresClientWitness,
                    RequiresHoldPoint = @RequiresHoldPoint
                  WHERE Id = @Id",
                new
                {
                    item.WeldType,

                    RequiredNdtType =
                        (int)item.RequiredNdtType,

                    item.InspectionPercentage,

                    RequiresClientWitness =
                        item.RequiresClientWitness ? 1 : 0,

                    RequiresHoldPoint =
                        item.RequiresHoldPoint ? 1 : 0,

                    Id =
                        item.Id.ToString()
                });
        }
    }
}
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class ReservedMaterialRepository
    {
        private readonly string _connectionString;

        public ReservedMaterialRepository()
        {
            var path = DatabasePath.Get();

            Debug.WriteLine(
                $"ReservedMaterialRepository: {path}");

            _connectionString =
                $"Data Source={path}";
        }

        public List<ReservedMaterial> GetAll()
        {
            var result =
                new List<ReservedMaterial>();

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
            SELECT
                Id,
                WorkOrderId,
                ItemCode,
                Quantity,
                ReservedOn
            FROM ReservedMaterials
            """;

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(
                    new ReservedMaterial
                    {
                        Id =
                            Guid.Parse(
                                reader.GetString(0)),

                        WorkOrderId =
                            Guid.Parse(
                                reader.GetString(1)),

                        ItemCode =
                            reader.GetString(2),

                        Quantity =
                            reader.GetDecimal(3),

                        ReservedOn =
                            DateTime.Parse(
                                reader.GetString(4))
                    });
            }

            return result;
        }

        public void Add(
            ReservedMaterial reservation)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
            INSERT INTO ReservedMaterials
            (
                Id,
                WorkOrderId,
                ItemCode,
                Quantity,
                ReservedOn
            )
            VALUES
            (
                $id,
                $workOrderId,
                $itemCode,
                $quantity,
                $reservedOn
            )
            """;

            command.Parameters.AddWithValue(
                "$id",
                reservation.Id.ToString());

            command.Parameters.AddWithValue(
                "$workOrderId",
                reservation.WorkOrderId.ToString());

            command.Parameters.AddWithValue(
                "$itemCode",
                reservation.ItemCode);

            command.Parameters.AddWithValue(
                "$quantity",
                reservation.Quantity);

            command.Parameters.AddWithValue(
                "$reservedOn",
                reservation.ReservedOn.ToString("O"));

            command.ExecuteNonQuery();
        }

        public void DeleteByWorkOrder(
            Guid workOrderId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
            DELETE
            FROM ReservedMaterials
            WHERE WorkOrderId = $id
            """;

            command.Parameters.AddWithValue(
                "$id",
                workOrderId.ToString());

            command.ExecuteNonQuery();
        }

        public decimal GetReservedQuantity(
    string itemCode)
        {
            return GetAll()
                .Where(x => x.ItemCode == itemCode)
                .Sum(x => x.Quantity);
        }

        public double GetAvailableQuantity(
            string itemCode,
            double physicalQuantity)
        {
            var reserved =
                (double)GetReservedQuantity(itemCode);

            return physicalQuantity - reserved;
        }
    }
}
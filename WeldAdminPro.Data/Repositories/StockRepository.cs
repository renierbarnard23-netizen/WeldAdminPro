using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class StockRepository
    {
        private readonly string _connectionString =
            "Data Source=weldadmin.db";

        public StockRepository()
        {
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS StockItems (" +
                "Id TEXT PRIMARY KEY, " +
                "ItemCode TEXT NOT NULL, " +
                "Description TEXT, " +
                "Quantity INTEGER NOT NULL, " +
                "Unit TEXT" +
                ");";

            cmd.ExecuteNonQuery();
        }

        public List<StockItem> GetAll()
        {
            var list = new List<StockItem>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Id, ItemCode, Description, Quantity, Unit FROM StockItems;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StockItem
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    ItemCode = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    Unit = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }

            return list;
        }

        public void Add(StockItem item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO StockItems (Id, ItemCode, Description, Quantity, Unit) " +
                "VALUES ($id, $code, $desc, $qty, $unit);";

            cmd.Parameters.AddWithValue("$id", item.Id.ToString());
            cmd.Parameters.AddWithValue("$code", item.ItemCode);
            cmd.Parameters.AddWithValue("$desc", item.Description);
            cmd.Parameters.AddWithValue("$qty", item.Quantity);
            cmd.Parameters.AddWithValue("$unit", item.Unit);

            cmd.ExecuteNonQuery();
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data;

namespace WeldAdminPro.Data.Repositories
{
    public class StockRepository
    {
        private readonly string _connectionString = $"Data Source={DatabasePath.Get()}";

		public StockRepository()
        {
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            // Stock items table
            cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS StockItems (" +
                "Id TEXT PRIMARY KEY, " +
                "ItemCode TEXT NOT NULL, " +
                "Description TEXT, " +
                "Quantity INTEGER NOT NULL, " +
                "Unit TEXT" +
                ");";
            cmd.ExecuteNonQuery();

            // Stock transactions table
            cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS StockTransactions (" +
                "Id TEXT PRIMARY KEY, " +
                "StockItemId TEXT NOT NULL, " +
                "TransactionDate TEXT NOT NULL, " +
                "Quantity INTEGER NOT NULL, " +
                "Type TEXT NOT NULL, " +
                "Reference TEXT" +
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
        public List<StockTransaction> GetAllTransactions()
{
    var list = new List<StockTransaction>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
        "SELECT " +
        "t.Id, " +
        "t.StockItemId, " +
        "t.TransactionDate, " +
        "t.Quantity, " +
        "t.Type, " +
        "t.Reference, " +
        "i.ItemCode, " +
        "i.Description " +
        "FROM StockTransactions t " +
        "JOIN StockItems i ON i.Id = t.StockItemId " +
        "ORDER BY t.TransactionDate DESC;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        list.Add(new StockTransaction
        {
            Id = Guid.Parse(reader.GetString(0)),
            StockItemId = Guid.Parse(reader.GetString(1)),
            TransactionDate = DateTime.Parse(reader.GetString(2)),
            Quantity = reader.GetInt32(3),
            Type = reader.GetString(4),
            Reference = reader.IsDBNull(5) ? "" : reader.GetString(5),

            // Convenience properties (do NOT affect DB)
            ItemCode = reader.GetString(6),
            ItemDescription = reader.GetString(7)
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

        public void Update(StockItem item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "UPDATE StockItems SET " +
                "Description = $desc, " +
                "Quantity = $qty, " +
                "Unit = $unit " +
                "WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", item.Id.ToString());
            cmd.Parameters.AddWithValue("$desc", item.Description);
            cmd.Parameters.AddWithValue("$qty", item.Quantity);
            cmd.Parameters.AddWithValue("$unit", item.Unit);

            cmd.ExecuteNonQuery();
        }

        public void AddTransaction(StockTransaction tx)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            // Insert transaction
            var txCmd = connection.CreateCommand();
            txCmd.CommandText =
                "INSERT INTO StockTransactions " +
                "(Id, StockItemId, TransactionDate, Quantity, Type, Reference) " +
                "VALUES ($id, $itemId, $date, $qty, $type, $ref);";

            txCmd.Parameters.AddWithValue("$id", tx.Id.ToString());
            txCmd.Parameters.AddWithValue("$itemId", tx.StockItemId.ToString());
            txCmd.Parameters.AddWithValue("$date", tx.TransactionDate.ToString("o"));
            txCmd.Parameters.AddWithValue("$qty", tx.Quantity);
            txCmd.Parameters.AddWithValue("$type", tx.Type);
            txCmd.Parameters.AddWithValue("$ref", tx.Reference);

            txCmd.ExecuteNonQuery();

            // Update stock balance
            var stockCmd = connection.CreateCommand();
            stockCmd.CommandText =
                "UPDATE StockItems SET Quantity = Quantity + $qty WHERE Id = $id;";

            stockCmd.Parameters.AddWithValue("$qty", tx.Quantity);
            stockCmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

            stockCmd.ExecuteNonQuery();

            transaction.Commit();
        }
    }
}

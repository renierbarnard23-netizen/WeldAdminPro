using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockRepository
	{
		private readonly string _connectionString = "Data Source=weldadmin.db";

		public StockRepository()
		{
			EnsureDatabase();
		}

		private void EnsureDatabase()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();

			// =========================
			// STOCK ITEMS TABLE
			// =========================
			cmd.CommandText =
				"CREATE TABLE IF NOT EXISTS StockItems (" +
				"Id TEXT PRIMARY KEY, " +
				"ItemCode TEXT NOT NULL, " +
				"Description TEXT, " +
				"Quantity INTEGER NOT NULL, " +
				"Unit TEXT, " +
				"Category TEXT NOT NULL DEFAULT 'Uncategorised', " +
				"MinLevel INTEGER NOT NULL DEFAULT 0, " +
				"MaxLevel INTEGER NOT NULL DEFAULT 0);";
			cmd.ExecuteNonQuery();

			// =========================
			// TRANSACTIONS TABLE
			// =========================
			cmd.CommandText =
				"CREATE TABLE IF NOT EXISTS StockTransactions (" +
				"Id TEXT PRIMARY KEY, " +
				"StockItemId TEXT NOT NULL, " +
				"TransactionDate TEXT NOT NULL, " +
				"Quantity INTEGER NOT NULL, " +
				"Type TEXT NOT NULL, " +
				"Reference TEXT);";
			cmd.ExecuteNonQuery();

			// =========================
			// SAFE MIGRATIONS
			// =========================
			TryAddColumn(cmd, "StockItems", "MinLevel", "INTEGER NOT NULL DEFAULT 0");
			TryAddColumn(cmd, "StockItems", "MaxLevel", "INTEGER NOT NULL DEFAULT 0");
		}

		private void TryAddColumn(SqliteCommand cmd, string table, string column, string definition)
		{
			cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";
			try { cmd.ExecuteNonQuery(); } catch { }
		}

		public bool HasTransactions(Guid stockItemId)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT COUNT(1) FROM StockTransactions WHERE StockItemId = $id;";
			cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

			long count = (long)cmd.ExecuteScalar();
			return count > 0;
		}

		// =========================
		// STOCK ITEMS
		// =========================

		public List<StockItem> GetAll()
		{
			var list = new List<StockItem>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, ItemCode, Description, Quantity, Unit, Category, MinLevel, MaxLevel " +
				"FROM StockItems;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new StockItem
				{
					Id = Guid.Parse(reader.GetString(0)),
					ItemCode = reader.GetString(1),
					Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
					Quantity = reader.GetInt32(3),
					Unit = reader.IsDBNull(4) ? "" : reader.GetString(4),
					Category = reader.IsDBNull(5) || string.IsNullOrWhiteSpace(reader.GetString(5))
						? "Uncategorised"
						: reader.GetString(5),
					MinLevel = reader.GetInt32(6),
					MaxLevel = reader.GetInt32(7)
				});
			}

			return list;
		}

		public StockItem GetById(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, ItemCode, Description, Quantity, Unit, Category, MinLevel, MaxLevel " +
				"FROM StockItems WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", id.ToString());

			using var reader = cmd.ExecuteReader();
			if (!reader.Read())
				throw new InvalidOperationException("Stock item not found.");

			return new StockItem
			{
				Id = Guid.Parse(reader.GetString(0)),
				ItemCode = reader.GetString(1),
				Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
				Quantity = reader.GetInt32(3),
				Unit = reader.IsDBNull(4) ? "" : reader.GetString(4),
				Category = reader.IsDBNull(5) || string.IsNullOrWhiteSpace(reader.GetString(5))
					? "Uncategorised"
					: reader.GetString(5),
				MinLevel = reader.GetInt32(6),
				MaxLevel = reader.GetInt32(7)
			};
		}

		public void Add(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT INTO StockItems " +
				"(Id, ItemCode, Description, Quantity, Unit, Category, MinLevel, MaxLevel) " +
				"VALUES ($id, $code, $desc, $qty, $unit, $cat, $min, $max);";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$code", item.ItemCode);
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");
			cmd.Parameters.AddWithValue("$min", item.MinLevel);
			cmd.Parameters.AddWithValue("$max", item.MaxLevel);

			cmd.ExecuteNonQuery();
		}

		public void Update(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE StockItems SET " +
				"ItemCode = $code, " +
				"Description = $desc, " +
				"Quantity = $qty, " +
				"Unit = $unit, " +
				"Category = $cat, " +
				"MinLevel = $min, " +
				"MaxLevel = $max " +
				"WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$code", item.ItemCode);
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");
			cmd.Parameters.AddWithValue("$min", item.MinLevel);
			cmd.Parameters.AddWithValue("$max", item.MaxLevel);

			cmd.ExecuteNonQuery();
		}

		public void DeleteStockItem(Guid itemId)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var checkCmd = connection.CreateCommand();
			checkCmd.CommandText =
				"SELECT COUNT(1) FROM StockTransactions WHERE StockItemId = $id;";
			checkCmd.Parameters.AddWithValue("$id", itemId.ToString());

			long transactionCount = (long)checkCmd.ExecuteScalar()!;

			if (transactionCount > 0)
				throw new InvalidOperationException(
					"This stock item cannot be deleted because it has transaction history.");

			var deleteCmd = connection.CreateCommand();
			deleteCmd.CommandText =
				"DELETE FROM StockItems WHERE Id = $id;";
			deleteCmd.Parameters.AddWithValue("$id", itemId.ToString());

			deleteCmd.ExecuteNonQuery();
		}

		// =========================
		// TRANSACTIONS
		// =========================

		public List<StockTransaction> GetAllTransactions()
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT t.Id, t.StockItemId, t.TransactionDate, t.Quantity, t.Type, t.Reference, " +
				"i.ItemCode, i.Description " +
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
					ItemCode = reader.GetString(6),
					ItemDescription = reader.GetString(7)
				});
			}

			return list;
		}

		public void AddTransaction(StockTransaction tx)
		{
			if (tx.Type == "OUT")
			{
				var item = GetById(tx.StockItemId);
				if (tx.Quantity > item.Quantity)
					throw new InvalidOperationException(
						$"Insufficient stock. Available: {item.Quantity}, Requested: {tx.Quantity}");
			}

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var transaction = connection.BeginTransaction();

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

			var stockCmd = connection.CreateCommand();
			stockCmd.CommandText =
				"UPDATE StockItems SET Quantity = Quantity + $delta WHERE Id = $id;";

			int delta = tx.Type == "IN" ? tx.Quantity : -tx.Quantity;
			stockCmd.Parameters.AddWithValue("$delta", delta);
			stockCmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

			stockCmd.ExecuteNonQuery();

			transaction.Commit();
		}
	}
}

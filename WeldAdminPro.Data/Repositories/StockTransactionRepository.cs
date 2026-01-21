using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockTransactionRepository
	{
		private readonly string _connectionString = "Data Source=weldadmin.db";

		public StockTransactionRepository()
		{
			EnsureDatabase();
		}

		// =========================
		// DATABASE INITIALISATION
		// =========================
		private void EnsureDatabase()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"CREATE TABLE IF NOT EXISTS StockTransactions (" +
				"Id TEXT PRIMARY KEY, " +
				"StockItemId TEXT NOT NULL, " +
				"TransactionDate TEXT NOT NULL, " +
				"Quantity INTEGER NOT NULL, " +
				"Type TEXT NOT NULL, " +
				"Reference TEXT);";

			cmd.ExecuteNonQuery();
		}

		// =========================
		// ADD TRANSACTION
		// =========================
		public void Add(StockTransaction transaction)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT INTO StockTransactions " +
				"(Id, StockItemId, TransactionDate, Quantity, Type, Reference) " +
				"VALUES ($id, $itemId, $date, $qty, $type, $ref);";

			cmd.Parameters.AddWithValue("$id", transaction.Id.ToString());
			cmd.Parameters.AddWithValue("$itemId", transaction.StockItemId.ToString());
			cmd.Parameters.AddWithValue("$date", transaction.TransactionDate.ToString("o"));
			cmd.Parameters.AddWithValue("$qty", transaction.Quantity);
			cmd.Parameters.AddWithValue("$type", transaction.Type);
			cmd.Parameters.AddWithValue("$ref", transaction.Reference ?? string.Empty);

			cmd.ExecuteNonQuery();
		}

		// =========================
		// GET TRANSACTIONS BY ITEM
		// =========================
		public IEnumerable<StockTransaction> GetByStockItem(Guid stockItemId)
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, StockItemId, TransactionDate, Quantity, Type, Reference " +
				"FROM StockTransactions " +
				"WHERE StockItemId = $id " +
				"ORDER BY TransactionDate DESC;";

			cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

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
					Reference = reader.GetString(5)
				});
			}

			return list;
		}
	}
}

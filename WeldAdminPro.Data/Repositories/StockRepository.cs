using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public partial class StockRepository
	{
		private readonly string _connectionString;

		public StockRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			// =========================
			// STOCK ITEMS
			// =========================
			cmd.CommandText = @"
				CREATE TABLE IF NOT EXISTS StockItems (
					Id TEXT PRIMARY KEY,
					ItemCode TEXT NOT NULL,
					Description TEXT,
					Quantity INTEGER NOT NULL,
					Unit TEXT,
					Category TEXT NOT NULL DEFAULT 'Uncategorised'
				);
			";
			cmd.ExecuteNonQuery();

			// =========================
			// TRANSACTIONS
			// =========================
			cmd.CommandText = @"
				CREATE TABLE IF NOT EXISTS StockTransactions (
					Id TEXT PRIMARY KEY,
					StockItemId TEXT NOT NULL,
					TransactionDate TEXT NOT NULL,
					Quantity INTEGER NOT NULL,
					Type TEXT NOT NULL,
					Reference TEXT
				);
			";
			cmd.ExecuteNonQuery();

			// =========================
			// 🔒 UNIQUE ITEM CODE GUARD
			// =========================
			EnsureUniqueItemCodeIndex(connection);
		}

		private void EnsureUniqueItemCodeIndex(SqliteConnection connection)
		{
			// Detect duplicates first
			using (var checkCmd = connection.CreateCommand())
			{
				checkCmd.CommandText = @"
					SELECT LOWER(ItemCode), COUNT(1)
					FROM StockItems
					GROUP BY LOWER(ItemCode)
					HAVING COUNT(1) > 1;
				";

				using var reader = checkCmd.ExecuteReader();
				if (reader.Read())
				{
					throw new InvalidOperationException(
						"Duplicate Item Codes detected in database.\n\n" +
						"Please resolve duplicates before continuing.\n\n" +
						"Item Codes must be unique (case-insensitive)."
					);
				}
			}

			// Create unique index (idempotent)
			using (var indexCmd = connection.CreateCommand())
			{
				indexCmd.CommandText = @"
					CREATE UNIQUE INDEX IF NOT EXISTS
					UX_StockItems_ItemCode
					ON StockItems (LOWER(ItemCode));
				";
				indexCmd.ExecuteNonQuery();
			}
		}

		// =========================
		// AUTO-SUGGEST NEXT ITEM CODE
		// =========================
		public string GetNextItemCodeSuggestion(int padLength = 3)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"SELECT ItemCode FROM StockItems;";

			int max = 0;

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var code = reader.GetString(0).Trim();
				if (int.TryParse(code, out int num) && num > max)
					max = num;
			}

			return (max + 1).ToString().PadLeft(padLength, '0');
		}

		// =========================
		// STOCK ITEMS
		// =========================
		public List<StockItem> GetAll()
		{
			var list = new List<StockItem>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				SELECT Id, ItemCode, Description, Quantity, Unit, Category
				FROM StockItems
				ORDER BY ItemCode;
			";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new StockItem
				{
					Id = Guid.Parse(reader.GetString(0)),
					ItemCode = reader.GetString(1),
					Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
					Quantity = reader.GetInt32(3),
					Unit = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
					Category = reader.IsDBNull(5) ? "Uncategorised" : reader.GetString(5)
				});
			}

			return list;
		}

		public void Add(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				INSERT INTO StockItems
				(Id, ItemCode, Description, Quantity, Unit, Category)
				VALUES
				($id, $code, $desc, $qty, $unit, $cat);
			";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$code", item.ItemCode.Trim());
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");

			cmd.ExecuteNonQuery();
		}

		public void Update(StockItem item)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				UPDATE StockItems SET
					Description = $desc,
					Unit = $unit,
					Category = $cat
				WHERE Id = $id;
			";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$cat", item.Category ?? "Uncategorised");

			cmd.ExecuteNonQuery();
		}

		// =========================
		// TRANSACTIONS
		// =========================
		public void AddTransaction(StockTransaction tx)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var dbTx = connection.BeginTransaction();

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
					INSERT INTO StockTransactions
					(Id, StockItemId, TransactionDate, Quantity, Type, Reference)
					VALUES
					($id, $itemId, $date, $qty, $type, $ref);
				";

				cmd.Parameters.AddWithValue("$id", tx.Id.ToString());
				cmd.Parameters.AddWithValue("$itemId", tx.StockItemId.ToString());
				cmd.Parameters.AddWithValue("$date", tx.TransactionDate.ToString("o"));
				cmd.Parameters.AddWithValue("$qty", tx.Quantity);
				cmd.Parameters.AddWithValue("$type", tx.Type);
				cmd.Parameters.AddWithValue("$ref", tx.Reference);

				cmd.ExecuteNonQuery();
			}

			int delta = tx.Type == "IN" ? tx.Quantity : -tx.Quantity;

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
					UPDATE StockItems
					SET Quantity = Quantity + $delta
					WHERE Id = $id;
				";

				cmd.Parameters.AddWithValue("$delta", delta);
				cmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

				cmd.ExecuteNonQuery();
			}

			dbTx.Commit();
		}

		// =========================
		// TRANSACTION HISTORY
		// =========================
		public List<StockTransaction> GetAllTransactions()
		{
			var list = new List<StockTransaction>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				SELECT
					t.Id,
					t.StockItemId,
					t.TransactionDate,
					t.Quantity,
					t.Type,
					t.Reference,
					i.ItemCode,
					i.Description
				FROM StockTransactions t
				JOIN StockItems i ON i.Id = t.StockItemId
				ORDER BY t.TransactionDate DESC;
			";

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
					Reference = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
					ItemCode = reader.GetString(6),
					ItemDescription = reader.GetString(7)
				});
			}

			return list;
		}
	}
}

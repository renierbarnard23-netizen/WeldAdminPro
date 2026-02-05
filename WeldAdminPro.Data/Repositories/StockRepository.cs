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

		// =========================
		// SCHEMA
		// =========================
		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StockItems (
	Id TEXT PRIMARY KEY,
	ItemCode TEXT NOT NULL,
	Description TEXT,
	Quantity INTEGER NOT NULL,
	Unit TEXT,
	MinLevel REAL NULL,
	MaxLevel REAL NULL,
	Category TEXT NOT NULL DEFAULT 'Uncategorised'
);";
			cmd.ExecuteNonQuery();

			TryAddColumn(cmd, "ALTER TABLE StockItems ADD COLUMN MinLevel REAL NULL;");
			TryAddColumn(cmd, "ALTER TABLE StockItems ADD COLUMN MaxLevel REAL NULL;");

			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StockTransactions (
	Id TEXT PRIMARY KEY,
	StockItemId TEXT NOT NULL,
	TransactionDate TEXT NOT NULL,
	Quantity INTEGER NOT NULL,
	Type TEXT NOT NULL,
	Reference TEXT
);";
			cmd.ExecuteNonQuery();

			EnsureUniqueItemCodeIndex(connection);
		}

		private void TryAddColumn(SqliteCommand cmd, string sql)
		{
			try { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
			catch (SqliteException) { }
		}

		private void EnsureUniqueItemCodeIndex(SqliteConnection connection)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
CREATE UNIQUE INDEX IF NOT EXISTS
UX_StockItems_ItemCode
ON StockItems (LOWER(ItemCode));";
			cmd.ExecuteNonQuery();
		}

		// =========================
		// AVAILABILITY (NEW)
		// =========================
		public int GetAvailableQuantity(Guid stockItemId)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Quantity FROM StockItems WHERE Id = $id;";
			cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

			var result = cmd.ExecuteScalar();
			return result == null ? 0 : Convert.ToInt32(result);
		}

		// =========================
		// ITEM CODE SUGGESTION
		// =========================
		public string GetNextItemCodeSuggestion()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT ItemCode
FROM StockItems
ORDER BY ItemCode DESC
LIMIT 1;";

			var result = cmd.ExecuteScalar()?.ToString();

			if (string.IsNullOrWhiteSpace(result))
				return "ITEM-001";

			var parts = result.Split('-', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2 || !int.TryParse(parts[^1], out int number))
				return result + "-1";

			return $"{string.Join('-', parts[..^1])}-{number + 1:000}";
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
SELECT Id, ItemCode, Description, Quantity, Unit, MinLevel, MaxLevel, Category
FROM StockItems
ORDER BY ItemCode;";

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
					MinLevel = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
					MaxLevel = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
					Category = reader.IsDBNull(7) ? "Uncategorised" : reader.GetString(7)
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
(Id, ItemCode, Description, Quantity, Unit, MinLevel, MaxLevel, Category)
VALUES
($id, $code, $desc, $qty, $unit, $min, $max, $cat);";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$code", item.ItemCode.Trim());
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$min", (object?)item.MinLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$max", (object?)item.MaxLevel ?? DBNull.Value);
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
	Quantity = $qty,
	Unit = $unit,
	MinLevel = $min,
	MaxLevel = $max,
	Category = $cat
WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity);
			cmd.Parameters.AddWithValue("$unit", item.Unit);
			cmd.Parameters.AddWithValue("$min", (object?)item.MinLevel ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$max", (object?)item.MaxLevel ?? DBNull.Value);
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
VALUES ($id, $itemId, $date, $qty, $type, $ref);";

				cmd.Parameters.AddWithValue("$id", tx.Id.ToString());
				cmd.Parameters.AddWithValue("$itemId", tx.StockItemId.ToString());
				cmd.Parameters.AddWithValue("$date", tx.TransactionDate.ToString("o"));
				cmd.Parameters.AddWithValue("$qty", tx.Quantity);
				cmd.Parameters.AddWithValue("$type", tx.Type);
				cmd.Parameters.AddWithValue("$ref", tx.Reference ?? string.Empty);

				cmd.ExecuteNonQuery();
			}

			int delta = tx.Type == "IN" ? tx.Quantity : -tx.Quantity;

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
UPDATE StockItems
SET Quantity = Quantity + $delta
WHERE Id = $id;";

				cmd.Parameters.AddWithValue("$delta", delta);
				cmd.Parameters.AddWithValue("$id", tx.StockItemId.ToString());

				cmd.ExecuteNonQuery();
			}

			dbTx.Commit();
		}

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
ORDER BY t.TransactionDate DESC;";

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

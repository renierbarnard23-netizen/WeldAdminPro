using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
	public class StockLedgerService
	{
		private readonly string _connectionString;

		public StockLedgerService()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
		}

		// =========================================================
		// FULL LEDGER REBUILD (RECALCULATE BalanceAfter)
		// =========================================================
		public void RebuildLedger()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var transaction = connection.BeginTransaction();

			try
			{
				var balances = new Dictionary<Guid, int>();

				using var selectCmd = connection.CreateCommand();
				selectCmd.Transaction = transaction;
				selectCmd.CommandText = @"
SELECT Id, StockItemId, Quantity, Type
FROM StockTransactions
ORDER BY TransactionDate ASC;";

				using var reader = selectCmd.ExecuteReader();

				var updates = new List<(Guid txId, int balanceAfter)>();

				while (reader.Read())
				{
					var txId = Guid.Parse(reader.GetString(0));
					var stockId = Guid.Parse(reader.GetString(1));
					var qty = reader.GetInt32(2);
					var type = reader.GetString(3);

					if (!balances.ContainsKey(stockId))
						balances[stockId] = 0;

					balances[stockId] += type == "IN" ? qty : -qty;

					updates.Add((txId, balances[stockId]));
				}

				reader.Close();

				foreach (var update in updates)
				{
					using var updateCmd = connection.CreateCommand();
					updateCmd.Transaction = transaction;
					updateCmd.CommandText =
						"UPDATE StockTransactions SET BalanceAfter=$bal WHERE Id=$id;";
					updateCmd.Parameters.AddWithValue("$bal", update.balanceAfter);
					updateCmd.Parameters.AddWithValue("$id", update.txId.ToString());
					updateCmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		// =========================================================
		// VERIFY STOCK INTEGRITY
		// =========================================================
		public List<string> ValidateLedgerIntegrity()
		{
			var errors = new List<string>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var ledgerBalances = new Dictionary<Guid, int>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
SELECT StockItemId, Quantity, Type
FROM StockTransactions
ORDER BY TransactionDate ASC;";

				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					var stockId = Guid.Parse(reader.GetString(0));
					var qty = reader.GetInt32(1);
					var type = reader.GetString(2);

					if (!ledgerBalances.ContainsKey(stockId))
						ledgerBalances[stockId] = 0;

					ledgerBalances[stockId] += type == "IN" ? qty : -qty;
				}
			}

			using (var stockCmd = connection.CreateCommand())
			{
				stockCmd.CommandText =
					"SELECT Id, ItemCode, Quantity FROM StockItems;";

				using var reader = stockCmd.ExecuteReader();

				while (reader.Read())
				{
					var id = Guid.Parse(reader.GetString(0));
					var itemCode = reader.GetString(1);
					var dbQty = reader.GetInt32(2);

					ledgerBalances.TryGetValue(id, out int ledgerQty);

					if (ledgerQty != dbQty)
					{
						errors.Add(
							$"Mismatch for {itemCode}: Ledger={ledgerQty}, StockItem={dbQty}");
					}
				}
			}

			return errors;
		}

		// =========================================================
		// GET STOCK POSITION AT DATE
		// =========================================================
		public int GetStockPositionAt(Guid stockItemId, DateTime date)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT Quantity, Type
FROM StockTransactions
WHERE StockItemId = $id
  AND TransactionDate <= $date
ORDER BY TransactionDate ASC;";

			cmd.Parameters.AddWithValue("$id", stockItemId.ToString());
			cmd.Parameters.AddWithValue("$date", date.ToString("o"));

			int balance = 0;

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var qty = reader.GetInt32(0);
				var type = reader.GetString(1);

				balance += type == "IN" ? qty : -qty;
			}

			return balance;
		}
	}
}

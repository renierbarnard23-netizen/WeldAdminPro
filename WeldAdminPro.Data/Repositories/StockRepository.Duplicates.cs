using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WeldAdminPro.Data.Repositories
{
	public partial class StockRepository
	{
		public List<DuplicateStockItem> GetDuplicateItemCodes()
		{
			var result = new List<DuplicateStockItem>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				SELECT Id, ItemCode
				FROM StockItems
				WHERE LOWER(ItemCode) IN (
					SELECT LOWER(ItemCode)
					FROM StockItems
					GROUP BY LOWER(ItemCode)
					HAVING COUNT(1) > 1
				)
				ORDER BY LOWER(ItemCode), Id;
			";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				result.Add(new DuplicateStockItem
				{
					Id = Guid.Parse(reader.GetString(0)),
					ItemCode = reader.GetString(1)
				});
			}

			return result;
		}

		public string GetNextAvailableItemCode(string baseCode)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			int number;
			if (!int.TryParse(baseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
				return $"{baseCode}-A";

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				SELECT MAX(CAST(ItemCode AS INTEGER))
				FROM StockItems
				WHERE ItemCode GLOB '[0-9]*';
			";

			var max = cmd.ExecuteScalar();
			var next = (max == DBNull.Value ? number : Convert.ToInt32(max)) + 1;

			return next.ToString("D3");
		}

		public void RenameItemCode(Guid itemId, string newCode, SqliteConnection connection)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				UPDATE StockItems
				SET ItemCode = $code
				WHERE Id = $id;
			";

			cmd.Parameters.AddWithValue("$code", newCode);
			cmd.Parameters.AddWithValue("$id", itemId.ToString());

			cmd.ExecuteNonQuery();
		}
	}

	public class DuplicateStockItem
	{
		public Guid Id { get; set; }
		public string ItemCode { get; set; } = string.Empty;
		public string ProposedItemCode { get; set; } = string.Empty;
	}
}

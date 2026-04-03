using Microsoft.Data.Sqlite;
using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProjectStockAllocationService
	{
		private readonly string _connectionString;

		public ProjectStockAllocationService()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ProjectStockAllocations (
	Id TEXT PRIMARY KEY,
	ProjectId TEXT NOT NULL,
	StockItemId TEXT NOT NULL,
	Quantity INTEGER NOT NULL,
	UnitCost REAL NOT NULL,
	AllocationDate TEXT NOT NULL,
	Reference TEXT
);";
			cmd.ExecuteNonQuery();
		}

		public void Allocate(Guid projectId, Guid stockItemId, int quantity, string reference = "")
		{
			if (quantity <= 0)
				throw new Exception("Allocation quantity must be greater than zero.");

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			using var tx = connection.BeginTransaction();

			// 1. Check stock + get cost
			decimal avgCost;
			int currentQty;

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
SELECT Quantity, AverageUnitCost
FROM StockItems
WHERE Id = $id;";

				cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

				using var reader = cmd.ExecuteReader();
				if (!reader.Read())
					throw new Exception("Stock item not found.");

				currentQty = reader.GetInt32(0);
				avgCost = reader.GetDecimal(1);
			}

			if (currentQty < quantity)
				throw new Exception("Not enough stock available.");

			// 2. Insert allocation record
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO ProjectStockAllocations
(Id, ProjectId, StockItemId, Quantity, UnitCost, AllocationDate, Reference)
VALUES ($id, $project, $stock, $qty, $cost, $date, $ref);";

				cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
				cmd.Parameters.AddWithValue("$project", projectId.ToString());
				cmd.Parameters.AddWithValue("$stock", stockItemId.ToString());
				cmd.Parameters.AddWithValue("$qty", quantity);
				cmd.Parameters.AddWithValue("$cost", avgCost);
				cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
				cmd.Parameters.AddWithValue("$ref", reference ?? string.Empty);

				cmd.ExecuteNonQuery();
			}

			// 3. Deduct stock
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
UPDATE StockItems
SET Quantity = Quantity - $qty
WHERE Id = $id;";

				cmd.Parameters.AddWithValue("$qty", quantity);
				cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

				cmd.ExecuteNonQuery();
			}

			// 4. Increase Project CommittedCost
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = @"
UPDATE Projects
SET CommittedCost = IFNULL(CommittedCost,0) + $cost
WHERE Id = $id;";

				cmd.Parameters.AddWithValue("$cost", quantity * avgCost);
				cmd.Parameters.AddWithValue("$id", projectId.ToString());

				cmd.ExecuteNonQuery();
			}

			tx.Commit();
		}
	}
}

using Microsoft.Data.Sqlite;
using System;
using WeldAdminPro.Core.Models;
using System.Collections.Generic;

namespace WeldAdminPro.Data.Repositories
{
	public class WorkOrderMaterialRepository
	{
		private readonly string _connectionString;

		public WorkOrderMaterialRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";

			EnsureTable();
		}
		public List<WorkOrderMaterial> GetByWorkOrderId(Guid workOrderId)
		{
			var list = new List<WorkOrderMaterial>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
SELECT Id, WorkOrderId, ItemCode, RequiredQuantity
FROM WorkOrderMaterials
WHERE WorkOrderId = $woId;";

			cmd.Parameters.AddWithValue("$woId", workOrderId.ToString());

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				list.Add(new WorkOrderMaterial
				{
					Id = Guid.Parse(reader.GetString(0)),
					WorkOrderId = Guid.Parse(reader.GetString(1)),
					ItemCode = reader.GetString(2),
					RequiredQuantity = reader.GetDouble(3)
				});
			}

			return list;
		}

		private void EnsureTable()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS WorkOrderMaterials
            (
                Id TEXT PRIMARY KEY,
                WorkOrderId TEXT NOT NULL,
                ItemCode TEXT NOT NULL,
                RequiredQuantity REAL NOT NULL
            );";

			cmd.ExecuteNonQuery();
		}

		public void Add(WorkOrderMaterial material)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
            INSERT INTO WorkOrderMaterials
            (Id, WorkOrderId, ItemCode, RequiredQuantity)

            VALUES

            (@Id, @WorkOrderId, @ItemCode, @RequiredQuantity);";

			cmd.Parameters.AddWithValue("@Id", material.Id.ToString());
			cmd.Parameters.AddWithValue("@WorkOrderId", material.WorkOrderId.ToString());
			cmd.Parameters.AddWithValue("@ItemCode", material.ItemCode);
			cmd.Parameters.AddWithValue("@RequiredQuantity", material.RequiredQuantity);

			cmd.ExecuteNonQuery();
		}
	}
}
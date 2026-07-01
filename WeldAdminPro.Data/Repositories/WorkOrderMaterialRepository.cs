using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class WorkOrderMaterialRepository
	{
		private static Dictionary<Guid, List<WorkOrderMaterial>> _cache = new();
		private readonly string _connectionString;

        public WorkOrderMaterialRepository()
        {
            var path = DatabasePath.Get();

            Debug.WriteLine(
                $"WorkOrderMaterialRepository: {path}");

            _connectionString =
                $"Data Source={path}";

            EnsureTable();
        }

        private static HashSet<Guid> _loggedWorkOrders = new();

        public List<WorkOrderMaterial>GetByWorkOrderId(Guid workOrderId)
        {
            var list =
                new List<WorkOrderMaterial>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
        SELECT Id, WorkOrderId, ItemCode, RequiredQuantity
        FROM WorkOrderMaterials
        WHERE WorkOrderId = @WorkOrderId";

			cmd.Parameters.AddWithValue("@WorkOrderId", workOrderId.ToString());

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

			// ✅ LOG ONLY ONCE PER WORK ORDER EVER
			if (!_loggedWorkOrders.Contains(workOrderId))
			{
				// Debug logging disabled for production stability
				// Debug.WriteLine($"🔥 MATERIAL COUNT: {list.Count}");
				_loggedWorkOrders.Add(workOrderId);
			}

			return list;
		}
        private void EnsureTable()
        {
            try
            {
                using var connection =
                    new SqliteConnection(_connectionString);

                connection.Open();

                using var cmd =
                    connection.CreateCommand();

                cmd.CommandText =
                """
        CREATE TABLE IF NOT EXISTS WorkOrderMaterials
        (
            Id TEXT PRIMARY KEY,
            WorkOrderId TEXT NOT NULL,
            ItemCode TEXT NOT NULL,
            RequiredQuantity REAL NOT NULL
        );
        """;

                cmd.ExecuteNonQuery();

                #if DEBUG
                Debug.WriteLine(
                    "✅ WorkOrderMaterials table ensured.");
                #endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"❌ Failed creating WorkOrderMaterials: {ex}");
            }
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

            _cache.Remove(
				material.WorkOrderId);
        }
	}
}
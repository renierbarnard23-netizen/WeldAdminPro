using Microsoft.Data.Sqlite;
using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class WorkOrderMaterialRepository
	{
		private readonly string _connectionString =
			$"Data Source={DatabasePath.Get()}";

		public void Add(WorkOrderMaterial material)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();

			cmd.CommandText = @"
INSERT INTO WorkOrderMaterials
(
    Id,
    WorkOrderId,
    ItemId,
    Quantity,
    IssuedOn
)
VALUES
(
    @Id,
    @WorkOrderId,
    @ItemId,
    @Quantity,
    @IssuedOn
);";

			cmd.Parameters.AddWithValue("@Id", material.Id.ToString());
			cmd.Parameters.AddWithValue("@WorkOrderId", material.WorkOrderId.ToString());
			cmd.Parameters.AddWithValue("@ItemId", material.ItemId.ToString());
			cmd.Parameters.AddWithValue("@Quantity", material.Quantity);
			cmd.Parameters.AddWithValue("@IssuedOn", material.IssuedOn.ToString("O"));

			cmd.ExecuteNonQuery();
		}
	}
}
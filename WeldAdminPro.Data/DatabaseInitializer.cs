using Microsoft.Data.Sqlite;
using WeldAdminPro.Data.Migrations;

namespace WeldAdminPro.Data
{
	public static class DatabaseInitializer
	{
		public static void Initialize(string connectionString)
		{
			using var connection = new SqliteConnection(connectionString);
			connection.Open();

			Migration_StockQuantityConstraint.Apply(connection);
		}
	}
}

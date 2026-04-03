using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class ProductionSettingsRepository
	{
		private readonly string _connectionString;

		public ProductionSettingsRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureTable();
		}

		private void EnsureTable()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"CREATE TABLE IF NOT EXISTS ProductionSettings
            (
                Id INTEGER PRIMARY KEY,
                Workers INTEGER,
                HoursPerDay REAL,
                OvertimeHours REAL,
                Shifts INTEGER
            );";

			cmd.ExecuteNonQuery();
		}

		public ProductionSettings Get()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = "SELECT * FROM ProductionSettings LIMIT 1";

			using var reader = cmd.ExecuteReader();

			if (reader.Read())
			{
				return new ProductionSettings
				{
					Workers = reader.GetInt32(1),
					HoursPerDay = reader.GetDouble(2),
					OvertimeHours = reader.GetDouble(3),
					Shifts = reader.GetInt32(4)
				};
			}

			var settings = new ProductionSettings();

			Save(settings);

			return settings;
		}

		public void Save(ProductionSettings settings)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText =
			@"DELETE FROM ProductionSettings";

			cmd.ExecuteNonQuery();

			cmd.CommandText =
			@"INSERT INTO ProductionSettings
            (Workers, HoursPerDay, OvertimeHours, Shifts)
            VALUES
            (@Workers, @Hours, @OT, @Shifts)";

			cmd.Parameters.AddWithValue("@Workers", settings.Workers);
			cmd.Parameters.AddWithValue("@Hours", settings.HoursPerDay);
			cmd.Parameters.AddWithValue("@OT", settings.OvertimeHours);
			cmd.Parameters.AddWithValue("@Shifts", settings.Shifts);

			cmd.ExecuteNonQuery();
		}
	}
}
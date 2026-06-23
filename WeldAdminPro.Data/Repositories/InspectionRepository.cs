using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class InspectionRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public void Add(Inspection inspection)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Inspection (Type, Result, Date, Inspector)
VALUES ($type, $result, $date, $inspector);";

            cmd.Parameters.AddWithValue("$type", inspection.Type);
            cmd.Parameters.AddWithValue("$result", inspection.Result);
            cmd.Parameters.AddWithValue("$date", inspection.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$inspector", inspection.Inspector);

            cmd.ExecuteNonQuery();
        }

        public List<Inspection> GetAll()
        {
            var list = new List<Inspection>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Inspection;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Inspection
                {
                    Id = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    Result = reader.GetString(2),
                    Date = DateTime.Parse(reader.GetString(3)),
                    Inspector = reader.GetString(4)
                });
            }

            return list;
        }

        public void Update(Inspection inspection)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Inspection
                SET Type = $type,
                    Result = $result,
                    Date = $date,
                    Inspector = $inspector
                WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", inspection.Id);
            cmd.Parameters.AddWithValue("$type", inspection.Type);
            cmd.Parameters.AddWithValue("$result", inspection.Result);
            cmd.Parameters.AddWithValue("$date", inspection.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$inspector", inspection.Inspector);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Inspection WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
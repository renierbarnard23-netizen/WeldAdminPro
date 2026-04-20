using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class WelderQualificationRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public void Add(WelderQualification wq)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO WelderQualification 
(WelderName, Process, Position, QualificationDate, ExpiryDate)
VALUES ($name, $process, $position, $qualDate, $expDate);";

            cmd.Parameters.AddWithValue("$name", wq.WelderName);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$position", wq.Position);
            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$expDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));

            cmd.ExecuteNonQuery();
        }

        public List<WelderQualification> GetAll()
        {
            var list = new List<WelderQualification>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM WelderQualification;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new WelderQualification
                {
                    Id = reader.GetInt32(0),
                    WelderName = reader.GetString(1),
                    Process = reader.GetString(2),
                    Position = reader.GetString(3),
                    QualificationDate = DateTime.Parse(reader.GetString(4)),
                    ExpiryDate = DateTime.Parse(reader.GetString(5))
                });
            }

            return list;
        }
        public List<WelderQualification> GetValidByProcess(string process)
        {
            var list = new List<WelderQualification>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
SELECT * FROM WelderQualification 
WHERE Process = $process;";
            cmd.Parameters.AddWithValue("$process", process);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var expiry = DateTime.Parse(reader.GetString(5));

                // 🔥 ISO RULE: Only valid (not expired)
                if (expiry >= DateTime.Today)
                {
                    list.Add(new WelderQualification
                    {
                        Id = reader.GetInt32(0),
                        WelderName = reader.GetString(1),
                        Process = reader.GetString(2),
                        Position = reader.GetString(3),
                        QualificationDate = DateTime.Parse(reader.GetString(4)),
                        ExpiryDate = expiry
                    });
                }
            }

            return list;
        }

        public void Update(WelderQualification wq)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE WelderQualification
                SET WelderName = $name,
                    Process = $process,
                    Position = $position,
                    QualificationDate = $qualDate,
                    ExpiryDate = $expDate
                WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", wq.Id);
            cmd.Parameters.AddWithValue("$name", wq.WelderName);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$position", wq.Position);
            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$expDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM WelderQualification WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class MaterialRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public void Add(Material material)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Material (MaterialGrade, HeatNumber, CertificateNumber)
VALUES ($grade, $heat, $cert);";

            cmd.Parameters.AddWithValue("$grade", material.MaterialGrade);
            cmd.Parameters.AddWithValue("$heat", material.HeatNumber);
            cmd.Parameters.AddWithValue("$cert", material.CertificateNumber);

            cmd.ExecuteNonQuery();
        }

        public List<Material> GetAll()
        {
            var list = new List<Material>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Material;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Material
                {
                    Id = reader.GetInt32(0),
                    MaterialGrade = reader.GetString(1),
                    HeatNumber = reader.GetString(2),
                    CertificateNumber = reader.GetString(3)
                });
            }

            return list;
        }

        public void Update(Material material)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Material
                SET MaterialGrade = $grade,
                    HeatNumber = $heat,
                    CertificateNumber = $cert
                WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", material.Id);
            cmd.Parameters.AddWithValue("$grade", material.MaterialGrade);
            cmd.Parameters.AddWithValue("$heat", material.HeatNumber);
            cmd.Parameters.AddWithValue("$cert", material.CertificateNumber);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Material WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
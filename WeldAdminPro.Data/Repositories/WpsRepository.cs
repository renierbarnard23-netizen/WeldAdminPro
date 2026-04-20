using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class WpsRepository : IWpsRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public void Add(Wps wps)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            if (wps.Id == Guid.Empty)
                wps.Id = Guid.NewGuid();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Wps 
                    (Id, WpsNumber, Process, MaterialGroup, ThicknessMin, ThicknessMax, PqrId, PNumber, FNumber, Position, JointType, Diameter)
                VALUES 
                    ($id, $wpsNumber, $process, $materialGroup, $tMin, $tMax, $pqrId, $pNumber, $fNumber, $position, $jointType, $diameter);";

            cmd.Parameters.AddWithValue("$id", wps.Id.ToString());
            cmd.Parameters.AddWithValue("$wpsNumber", wps.WpsNumber);
            cmd.Parameters.AddWithValue("$process", wps.Process);
            cmd.Parameters.AddWithValue("$materialGroup", wps.MaterialGroup);
            cmd.Parameters.AddWithValue("$tMin", wps.ThicknessMin);
            cmd.Parameters.AddWithValue("$tMax", wps.ThicknessMax);
            cmd.Parameters.AddWithValue("$pqrId", (object?)wps.PqrId?.ToString() ?? DBNull.Value);

            // 🔥 NEW FIELDS
            cmd.Parameters.AddWithValue("$pNumber", (object?)wps.PNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$fNumber", (object?)wps.FNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$position", (object?)wps.Position ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$jointType", (object?)wps.JointType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$diameter", wps.Diameter);

            cmd.ExecuteNonQuery();
        }

        public List<Wps> GetAll()
        {
            var list = new List<Wps>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Wps;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Guid id = Guid.Empty;
                var idValue = reader["Id"]?.ToString();
                if (!string.IsNullOrWhiteSpace(idValue))
                    Guid.TryParse(idValue, out id);

                Guid? pqrId = null;
                var pqrValue = reader["PqrId"]?.ToString();
                if (!string.IsNullOrWhiteSpace(pqrValue) && Guid.TryParse(pqrValue, out var parsedPqr))
                    pqrId = parsedPqr;

                list.Add(new Wps
                {
                    Id = id,

                    WpsNumber = reader["WpsNumber"]?.ToString() ?? "",
                    Process = reader["Process"]?.ToString() ?? "",
                    MaterialGroup = reader["MaterialGroup"]?.ToString() ?? "",

                    ThicknessMin = reader["ThicknessMin"] != DBNull.Value
                        ? Convert.ToDouble(reader["ThicknessMin"])
                        : 0,

                    ThicknessMax = reader["ThicknessMax"] != DBNull.Value
                        ? Convert.ToDouble(reader["ThicknessMax"])
                        : 0,

                    PqrId = pqrId,

                    PNumber = reader["PNumber"] == DBNull.Value ? null : reader["PNumber"].ToString(),
                    FNumber = reader["FNumber"] == DBNull.Value ? null : reader["FNumber"].ToString(),
                    Position = reader["Position"] == DBNull.Value ? null : reader["Position"].ToString(),
                    JointType = reader["JointType"] == DBNull.Value ? null : reader["JointType"].ToString(),

                    Diameter = reader["Diameter"] != DBNull.Value
                        ? Convert.ToDouble(reader["Diameter"])
                        : 0
                });
            }

            return list;
        }

        public Wps? GetByWpsNumber(string wpsNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Wps WHERE WpsNumber = $wpsNumber LIMIT 1";
            cmd.Parameters.AddWithValue("$wpsNumber", wpsNumber);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return Map(reader); // ✅ now this exists
            }

            return null;
        }

        private Wps Map(SqliteDataReader reader)
        {
            Guid id = Guid.Empty;
            var idValue = reader["Id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(idValue))
                Guid.TryParse(idValue, out id);

            Guid? pqrId = null;
            var pqrValue = reader["PqrId"]?.ToString();
            if (!string.IsNullOrWhiteSpace(pqrValue) && Guid.TryParse(pqrValue, out var parsedPqr))
                pqrId = parsedPqr;

            return new Wps
            {
                Id = id,

                WpsNumber = reader["WpsNumber"]?.ToString() ?? "",
                Process = reader["Process"]?.ToString() ?? "",
                MaterialGroup = reader["MaterialGroup"]?.ToString() ?? "",

                ThicknessMin = reader["ThicknessMin"] != DBNull.Value
                    ? Convert.ToDouble(reader["ThicknessMin"])
                    : 0,

                ThicknessMax = reader["ThicknessMax"] != DBNull.Value
                    ? Convert.ToDouble(reader["ThicknessMax"])
                    : 0,

                PqrId = pqrId,

                PNumber = reader["PNumber"] == DBNull.Value ? null : reader["PNumber"].ToString(),
                FNumber = reader["FNumber"] == DBNull.Value ? null : reader["FNumber"].ToString(),
                Position = reader["Position"] == DBNull.Value ? null : reader["Position"].ToString(),
                JointType = reader["JointType"] == DBNull.Value ? null : reader["JointType"].ToString(),

                Diameter = reader["Diameter"] != DBNull.Value
                    ? Convert.ToDouble(reader["Diameter"])
                    : 0
            };
        }

        public void Update(Wps wps)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE Wps
SET WpsNumber = $wpsNumber,
    Process = $process,
    MaterialGroup = $materialGroup,
    ThicknessMin = $tMin,
    ThicknessMax = $tMax,
    PqrId = $pqrId,
    PNumber = $pNumber,
    FNumber = $fNumber,
    Position = $position,
    JointType = $jointType,
    Diameter = $diameter
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", wps.Id.ToString());
            cmd.Parameters.AddWithValue("$wpsNumber", wps.WpsNumber);
            cmd.Parameters.AddWithValue("$process", wps.Process);
            cmd.Parameters.AddWithValue("$materialGroup", wps.MaterialGroup);
            cmd.Parameters.AddWithValue("$tMin", wps.ThicknessMin);
            cmd.Parameters.AddWithValue("$tMax", wps.ThicknessMax);
            cmd.Parameters.AddWithValue("$pqrId", (object?)wps.PqrId?.ToString() ?? DBNull.Value);

            // 🔥 NEW FIELDS
            cmd.Parameters.AddWithValue("$pNumber", (object?)wps.PNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$fNumber", (object?)wps.FNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$position", (object?)wps.Position ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$jointType", (object?)wps.JointType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$diameter", wps.Diameter);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Wps WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        private Guid SafeGuid(object value)
        {
            return Guid.TryParse(value?.ToString(), out var g) ? g : Guid.Empty;
        }

    }
}
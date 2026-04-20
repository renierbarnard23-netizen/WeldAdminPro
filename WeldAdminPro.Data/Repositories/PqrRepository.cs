using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class PqrRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        // =========================
        // ADD
        // =========================
        public void Add(Pqr pqr)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Pqr (
    Id, PqrNumber, QualificationDate, QualifiedBy, ThicknessTested,
    Process, MaterialGroup, Position,
    FillerMaterial, GasType,
    AmpsUsed, VoltsUsed,
    HeatInput, Preheat, Interpass,
    PwhtPerformed, WpsId,

    PNumber, FNumber, QualifiedPosition, JointType,
    ThicknessQualifiedMin, ThicknessQualifiedMax,
    DiameterMin, DiameterMax,
    WpsReferenceNumber
)
VALUES (
    $id, $number, $date, $by, $thickness,
    $process, $material, $position,
    $filler, $gas,
    $amps, $volts,
    $heat, $preheat, $interpass,
    $pwht, $wpsId,

    $pNumber, $fNumber, $qPosition, $joint,
    $tMin, $tMax,
    $dMin, $dMax,
    $wpsRef
);";

            cmd.Parameters.AddWithValue("$id", pqr.Id.ToString());
            cmd.Parameters.AddWithValue("$number", pqr.PqrNumber ?? "");
            cmd.Parameters.AddWithValue("$date", pqr.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$by", pqr.QualifiedBy ?? "");
            cmd.Parameters.AddWithValue("$thickness", pqr.ThicknessTested);

            cmd.Parameters.AddWithValue("$process", pqr.Process ?? "");
            cmd.Parameters.AddWithValue("$material", pqr.MaterialGroup ?? "");
            cmd.Parameters.AddWithValue("$position", pqr.Position ?? "");

            cmd.Parameters.AddWithValue("$filler", pqr.FillerMaterial ?? "");
            cmd.Parameters.AddWithValue("$gas", pqr.GasType ?? "");

            cmd.Parameters.AddWithValue("$amps", pqr.AmpsUsed);
            cmd.Parameters.AddWithValue("$volts", pqr.VoltsUsed);

            cmd.Parameters.AddWithValue("$heat", pqr.HeatInput);
            cmd.Parameters.AddWithValue("$preheat", pqr.Preheat);
            cmd.Parameters.AddWithValue("$interpass", pqr.Interpass);

            cmd.Parameters.AddWithValue("$pwht", pqr.PwhtPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("$wpsId", (object?)pqr.WpsId?.ToString() ?? DBNull.Value);

            // ESSENTIAL VARIABLES
            cmd.Parameters.AddWithValue("$pNumber", (object?)pqr.PNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$fNumber", (object?)pqr.FNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$qPosition", (object?)pqr.QualifiedPosition ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$joint", (object?)pqr.JointType ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$tMin", pqr.ThicknessQualifiedMin);
            cmd.Parameters.AddWithValue("$tMax", pqr.ThicknessQualifiedMax);

            cmd.Parameters.AddWithValue("$dMin", pqr.DiameterMin);
            cmd.Parameters.AddWithValue("$dMax", pqr.DiameterMax);

            cmd.Parameters.AddWithValue("$wpsRef", (object?)pqr.WpsReferenceNumber ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // UPDATE
        // =========================
        public void Update(Pqr pqr)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE Pqr SET
    PqrNumber = $number,
    QualificationDate = $date,
    QualifiedBy = $by,
    ThicknessTested = $thickness,

    Process = $process,
    MaterialGroup = $material,
    Position = $position,

    FillerMaterial = $filler,
    GasType = $gas,

    AmpsUsed = $amps,
    VoltsUsed = $volts,
    HeatInput = $heat,

    Preheat = $preheat,
    Interpass = $interpass,
    PwhtPerformed = $pwht,
    WpsId = $wpsId,

    PNumber = $pNumber,
    FNumber = $fNumber,
    QualifiedPosition = $qPosition,
    JointType = $joint,

    ThicknessQualifiedMin = $tMin,
    ThicknessQualifiedMax = $tMax,

    DiameterMin = $dMin,
    DiameterMax = $dMax,

    WpsReferenceNumber = $wpsRef

WHERE Id = $id;
";

            cmd.Parameters.AddWithValue("$id", pqr.Id.ToString());
            cmd.Parameters.AddWithValue("$number", pqr.PqrNumber ?? "");
            cmd.Parameters.AddWithValue("$date", pqr.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$by", pqr.QualifiedBy ?? "");
            cmd.Parameters.AddWithValue("$thickness", pqr.ThicknessTested);

            cmd.Parameters.AddWithValue("$process", pqr.Process ?? "");
            cmd.Parameters.AddWithValue("$material", pqr.MaterialGroup ?? "");
            cmd.Parameters.AddWithValue("$position", pqr.Position ?? "");

            cmd.Parameters.AddWithValue("$filler", pqr.FillerMaterial ?? "");
            cmd.Parameters.AddWithValue("$gas", pqr.GasType ?? "");

            cmd.Parameters.AddWithValue("$amps", pqr.AmpsUsed);
            cmd.Parameters.AddWithValue("$volts", pqr.VoltsUsed);
            cmd.Parameters.AddWithValue("$heat", pqr.HeatInput);

            cmd.Parameters.AddWithValue("$preheat", pqr.Preheat);
            cmd.Parameters.AddWithValue("$interpass", pqr.Interpass);
            cmd.Parameters.AddWithValue("$pwht", pqr.PwhtPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("$wpsId", (object?)pqr.WpsId?.ToString() ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$pNumber", (object?)pqr.PNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$fNumber", (object?)pqr.FNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$qPosition", (object?)pqr.QualifiedPosition ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$joint", (object?)pqr.JointType ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$tMin", pqr.ThicknessQualifiedMin);
            cmd.Parameters.AddWithValue("$tMax", pqr.ThicknessQualifiedMax);

            cmd.Parameters.AddWithValue("$dMin", pqr.DiameterMin);
            cmd.Parameters.AddWithValue("$dMax", pqr.DiameterMax);

            cmd.Parameters.AddWithValue("$wpsRef", (object?)pqr.WpsReferenceNumber ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // GET BY NUMBER
        // =========================
        public Pqr? GetByNumber(string? pqrNumber)
        {
            if (string.IsNullOrWhiteSpace(pqrNumber))
                return null; // 🔥 prevent crash

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pqr WHERE PqrNumber = $num;";
            cmd.Parameters.AddWithValue("$num", pqrNumber);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        // =========================
        // GET ALL
        // =========================
        public List<Pqr> GetAll()
        {
            var list = new List<Pqr>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pqr;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        // =========================
        // DELETE
        // =========================
        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Pqr WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        // =========================
        // MAPPER (SAFE)
        // =========================
        private Pqr Map(SqliteDataReader reader)
        {
            return new Pqr
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                PqrNumber = reader["PqrNumber"]?.ToString() ?? "",
                QualificationDate = DateTime.Parse(reader["QualificationDate"].ToString()!),
                QualifiedBy = reader["QualifiedBy"]?.ToString() ?? "",
                ThicknessTested = reader["ThicknessTested"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessTested"]),

                Process = reader["Process"]?.ToString() ?? "",
                MaterialGroup = reader["MaterialGroup"]?.ToString() ?? "",
                Position = reader["Position"]?.ToString() ?? "",

                FillerMaterial = reader["FillerMaterial"]?.ToString() ?? "",
                GasType = reader["GasType"]?.ToString() ?? "",

                AmpsUsed = reader["AmpsUsed"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AmpsUsed"]),
                VoltsUsed = reader["VoltsUsed"] == DBNull.Value ? 0 : Convert.ToDouble(reader["VoltsUsed"]),
                HeatInput = reader["HeatInput"] == DBNull.Value ? 0 : Convert.ToDouble(reader["HeatInput"]),

                Preheat = reader["Preheat"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Preheat"]),
                Interpass = reader["Interpass"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Interpass"]),
                PwhtPerformed = reader["PwhtPerformed"] != DBNull.Value && Convert.ToInt32(reader["PwhtPerformed"]) == 1,

                WpsId = reader["WpsId"] == DBNull.Value ? null : Guid.Parse(reader["WpsId"].ToString()!),

                // ESSENTIAL
                PNumber = reader["PNumber"]?.ToString() ?? "",
                FNumber = reader["FNumber"]?.ToString() ?? "",
                QualifiedPosition = reader["QualifiedPosition"]?.ToString() ?? "",
                JointType = reader["JointType"]?.ToString() ?? "",

                ThicknessQualifiedMin = reader["ThicknessQualifiedMin"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessQualifiedMin"]),
                ThicknessQualifiedMax = reader["ThicknessQualifiedMax"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessQualifiedMax"]),

                DiameterMin = reader["DiameterMin"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiameterMin"]),
                DiameterMax = reader["DiameterMax"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiameterMax"]),

                WpsReferenceNumber = reader["WpsReferenceNumber"]?.ToString() ?? ""
            };
        }
    }
}
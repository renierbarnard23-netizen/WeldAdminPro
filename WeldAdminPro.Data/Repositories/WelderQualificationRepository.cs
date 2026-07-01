using System.Linq;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class WelderQualificationRepository
    : IWelderQualificationRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public void Add(WelderQualification wq)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WelderQualification 
            (
                    WelderNumber,
                    Process,
                    MaterialGroup,
                    Position,
                    ThicknessMin,
                    ThicknessMax,
                    QualificationDate,
                    InitialQualificationDate,
                    RenewalDate,
                    ExpiryDate
            )
                VALUES
                (
                    $name,
                    $process,
                    $material,
                    $position,
                    $tmin,
                    $tmax,
                    $qualDate,
                    $initialDate,
                    $renewalDate,
                    $expDate
            );";

            cmd.Parameters.AddWithValue("$name", wq.WelderNumber);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$material", wq.MaterialGroup);
            cmd.Parameters.AddWithValue("$tmin", wq.ThicknessMin);
            cmd.Parameters.AddWithValue("$tmax", wq.ThicknessMax);

            cmd.Parameters.AddWithValue("$position", wq.Position);
            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$expDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue("$initialDate", wq.InitialQualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$renewalDate", wq.RenewalDate == null ? DBNull.Value : wq.RenewalDate.Value.ToString("yyyy-MM-dd"));

            cmd.ExecuteNonQuery();
        }

        public WelderQualification? GetByWelderNumber(
    string welderNumber)
        {
            var qualifications = GetAll();

            return qualifications
                .FirstOrDefault(x =>
                    x.WelderNumber == welderNumber);
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
                    WelderNumber = reader.GetString(1),
                    Process = reader.GetString(2),
                    MaterialGroup = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Position = reader.GetString(4),

                    ThicknessMin = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                    ThicknessMax = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),

                    QualificationDate = DateTime.Parse(reader.GetString(7)),
                    ExpiryDate = DateTime.Parse(reader.GetString(10)),

                    InitialQualificationDate = string.IsNullOrWhiteSpace(reader["InitialQualificationDate"]?.ToString())
                        ? DateTime.MinValue
                        : DateTime.Parse(reader["InitialQualificationDate"]!.ToString()!),

                    RenewalDate = string.IsNullOrWhiteSpace(reader["RenewalDate"]?.ToString())
                        ? null
                        : DateTime.Parse(reader["RenewalDate"]!.ToString()!),
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
                var expiry = DateTime.Parse(reader.GetString(10));

                // 🔥 ISO RULE: Only valid (not expired)
                if (expiry >= DateTime.Today)
                {
                    list.Add(new WelderQualification
                    {
                        Id = reader.GetInt32(0),
                        WelderNumber = reader.GetString(1),
                        Process = reader.GetString(2),
                        MaterialGroup = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Position = reader.GetString(4),
                        ThicknessMin = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                        ThicknessMax = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                        QualificationDate = DateTime.Parse(reader.GetString(7)),
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
            SET
                WelderNumber = $welder,
                Process = $process,
                MaterialGroup = $material,
                Position = $position,

                ThicknessMin = $tmin,
                ThicknessMax = $tmax,

                QualificationDate = $qualDate,
                RenewalDate = $renewalDate,
                ExpiryDate = $expiryDate

            WHERE Id = $id";

            cmd.Parameters.AddWithValue("$welder", wq.WelderNumber);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$material", wq.MaterialGroup);
            cmd.Parameters.AddWithValue("$position", wq.Position);

            cmd.Parameters.AddWithValue("$tmin", wq.ThicknessMin);
            cmd.Parameters.AddWithValue("$tmax", wq.ThicknessMax);

            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue("$renewalDate", wq.RenewalDate?.ToString("yyyy-MM-dd") ?? "");

            cmd.Parameters.AddWithValue("$expiryDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue("$id", wq.Id);

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

        public bool HasValidQualification(string welderNumber)
        {
            var qualifications =
                GetAll()
                .Where(x =>
                    x.WelderNumber == welderNumber &&
                    x.ExpiryDate >= DateTime.Today)
                .ToList();

            return qualifications.Any();
        }

        public bool HasValidProcessQualification(
    string welderNumber,
    string process)
        {
            return GetAll().Any(x =>
                x.WelderNumber == welderNumber &&
                x.Process == process &&
                x.ExpiryDate >= DateTime.Today);
        }

        public List<WelderQualification> GetQualifications(
    string welderNumber,
    string process,
    string materialGroup,
    string position)
        {
            var normalizedProcess =
                process.Trim().ToUpper();

            if (normalizedProcess == "TIG")
                normalizedProcess = "GTAW";

            if (normalizedProcess == "MIG")
                normalizedProcess = "GMAW";

            var qualifications =
                GetAll()
                .Where(q =>
                    q.WelderNumber.Trim().ToUpper()
                        == welderNumber.Trim().ToUpper()

                    && q.IsActive

                    && q.ExpiryDate >= DateTime.Today

                    && NormalizeProcess(q.Process)
                        == normalizedProcess)
                .ToList();
            #if DEBUG
            Console.WriteLine($"Repository returned {qualifications.Count} qualifications for welder {welderNumber}");
            #endif

            foreach (var q in qualifications)
            {
                Console.WriteLine(
                    $"Welder={q.WelderNumber}, " +
                    $"Material={q.MaterialGroup}, " +
                    $"Position={q.Position}, " +
                    $"Process={q.Process}");
            }

            return qualifications;
        }

        private string NormalizeProcess(string process)
        {
            if (string.IsNullOrWhiteSpace(process))
                return "";

            process =
                process.Trim().ToUpper();

            if (process.Contains("TIG") ||
                process.Contains("GTAW"))
            {
                return "GTAW";
            }

            if (process.Contains("MIG") ||
                process.Contains("GMAW"))
            {
                return "GMAW";
            }

            if (process.Contains("STICK") ||
                process.Contains("SMAW"))
            {
                return "SMAW";
            }

            return process;
        }
                    
        }
    }

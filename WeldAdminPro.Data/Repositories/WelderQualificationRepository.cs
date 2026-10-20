using System.Linq;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class WelderQualificationRepository
    : IWelderQualificationRepository
    {
        private readonly string _databasePath;

        private string _connectionString => $"Data Source={_databasePath}";

        public WelderQualificationRepository()
            : this(DatabasePath.Get())
        {
        }

        public WelderQualificationRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException(
                    "Database path is required.",
                    nameof(databasePath));

            _databasePath = System.IO.Path.GetFullPath(databasePath);

            var directory = System.IO.Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
                System.IO.Directory.CreateDirectory(directory);
        }

        public void Add(WelderQualification wq)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WelderQualification 
            (
                    CompanyId,
                    WelderNumber,
                    Process,
                    MaterialGroup,
                    Position,
                    ThicknessMin,
                    ThicknessMax,
                    QualificationDate,
                    InitialQualificationDate,
                    RenewalDate,
                    ExpiryDate,
                    IsActive,
                    LastRenewedByUserId,
                    LastRenewedByName,
                    LastRenewedAt,
                    PreviousExpiryDate
            )
                VALUES
                (
                    $CompanyId,
                    $name,
                    $process,
                    $material,
                    $position,
                    $tmin,
                    $tmax,
                    $qualDate,
                    $initialDate,
                    $renewalDate,
                    $expDate,
                    $isActive,
                    $lastRenewedByUserId,
                    $lastRenewedByName,
                    $lastRenewedAt,
                    $previousExpiryDate
            );";

            cmd.Parameters.AddWithValue("$CompanyId",
                wq.CompanyId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$name", wq.WelderNumber);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$material", wq.MaterialGroup);
            cmd.Parameters.AddWithValue("$tmin", wq.ThicknessMin);
            cmd.Parameters.AddWithValue("$tmax", wq.ThicknessMax);

            cmd.Parameters.AddWithValue("$position", wq.Position);
            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$expDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$isActive", wq.IsActive ? 1 : 0);

            cmd.Parameters.AddWithValue("$initialDate", wq.InitialQualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$renewalDate", wq.RenewalDate == null ? DBNull.Value : wq.RenewalDate.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$lastRenewedByUserId", (object?)wq.LastRenewedByUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lastRenewedByName", (object?)wq.LastRenewedByName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lastRenewedAt", wq.LastRenewedAt.HasValue ? wq.LastRenewedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$previousExpiryDate", wq.PreviousExpiryDate.HasValue ? wq.PreviousExpiryDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);

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

        public List<WelderQualification> GetByCompany(Guid companyId)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException("Company Id is required.", nameof(companyId));

            return GetAll()
                .Where(x => x.CompanyId == companyId)
                .ToList();
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

                    CompanyId =
                        reader["CompanyId"] == DBNull.Value
                            ? null
                            : Guid.Parse(reader["CompanyId"].ToString()!),

                    IsActive =
                        reader["IsActive"] != DBNull.Value &&
                        Convert.ToInt32(reader["IsActive"]) == 1,
                    LastRenewedByUserId = reader["LastRenewedByUserId"] == DBNull.Value ? null : reader["LastRenewedByUserId"].ToString(),
                    LastRenewedByName = reader["LastRenewedByName"] == DBNull.Value ? null : reader["LastRenewedByName"].ToString(),
                    LastRenewedAt = reader["LastRenewedAt"] == DBNull.Value ? null : DateTime.Parse(reader["LastRenewedAt"].ToString()!),
                    PreviousExpiryDate = reader["PreviousExpiryDate"] == DBNull.Value ? null : DateTime.Parse(reader["PreviousExpiryDate"].ToString()!),
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

                // ðŸ”¥ ISO RULE: Only valid (not expired)
                if (reader["IsActive"] != DBNull.Value &&
    Convert.ToInt32(reader["IsActive"]) == 1 &&
    expiry >= DateTime.Today)
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
                        ExpiryDate = expiry,

                        CompanyId =
                            reader["CompanyId"] == DBNull.Value
                                ? null
                                : Guid.Parse(reader["CompanyId"].ToString()!),

                    IsActive =
                        reader["IsActive"] != DBNull.Value &&
                        Convert.ToInt32(reader["IsActive"]) == 1,
                    LastRenewedByUserId = reader["LastRenewedByUserId"] == DBNull.Value ? null : reader["LastRenewedByUserId"].ToString(),
                    LastRenewedByName = reader["LastRenewedByName"] == DBNull.Value ? null : reader["LastRenewedByName"].ToString(),
                    LastRenewedAt = reader["LastRenewedAt"] == DBNull.Value ? null : DateTime.Parse(reader["LastRenewedAt"].ToString()!),
                    PreviousExpiryDate = reader["PreviousExpiryDate"] == DBNull.Value ? null : DateTime.Parse(reader["PreviousExpiryDate"].ToString()!),
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
                CompanyId = $CompanyId,
                WelderNumber = $welder,
                Process = $process,
                MaterialGroup = $material,
                Position = $position,

                ThicknessMin = $tmin,
                ThicknessMax = $tmax,

                QualificationDate = $qualDate,
                RenewalDate = $renewalDate,
                ExpiryDate = $expiryDate,
                IsActive = $isActive,
                LastRenewedByUserId = $lastRenewedByUserId,
                LastRenewedByName = $lastRenewedByName,
                LastRenewedAt = $lastRenewedAt,
                PreviousExpiryDate = $previousExpiryDate

            WHERE Id = $id";

            cmd.Parameters.AddWithValue("$CompanyId",
                wq.CompanyId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$welder", wq.WelderNumber);
            cmd.Parameters.AddWithValue("$process", wq.Process);
            cmd.Parameters.AddWithValue("$material", wq.MaterialGroup);
            cmd.Parameters.AddWithValue("$position", wq.Position);

            cmd.Parameters.AddWithValue("$tmin", wq.ThicknessMin);
            cmd.Parameters.AddWithValue("$tmax", wq.ThicknessMax);

            cmd.Parameters.AddWithValue("$qualDate", wq.QualificationDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue("$renewalDate", wq.RenewalDate?.ToString("yyyy-MM-dd") ?? "");

            cmd.Parameters.AddWithValue("$expiryDate", wq.ExpiryDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$isActive", wq.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$lastRenewedByUserId", (object?)wq.LastRenewedByUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lastRenewedByName", (object?)wq.LastRenewedByName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lastRenewedAt", wq.LastRenewedAt.HasValue ? wq.LastRenewedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$previousExpiryDate", wq.PreviousExpiryDate.HasValue ? wq.PreviousExpiryDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);

            cmd.Parameters.AddWithValue("$id", wq.Id);

            cmd.ExecuteNonQuery();
        }

        public void RenewWithHistory(
            int qualificationId,
            Guid? companyId,
            string welderNumber,
            DateTime previousExpiryDate,
            DateTime newExpiryDate,
            DateTime renewalDate,
            int renewalPeriodMonths,
            string renewedByUserId,
            string? renewedByUsername,
            string renewedByName,
            string? lastRenewedByUserId,
            string? lastRenewedByName,
            DateTime? lastRenewedAt,
            DateTime? previousExpiryDateAudit)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var updateCommand = connection.CreateCommand();

                updateCommand.Transaction = transaction;

                updateCommand.CommandText = @"
                    UPDATE WelderQualification
                    SET
                        RenewalDate = $renewalDate,
                        ExpiryDate = $expiryDate,
                        IsActive = 1,
                        LastRenewedByUserId = $lastRenewedByUserId,
                        LastRenewedByName = $lastRenewedByName,
                        LastRenewedAt = $lastRenewedAt,
                        PreviousExpiryDate = $previousExpiryDate
                    WHERE Id = $id;
                ";

                updateCommand.Parameters.AddWithValue(
                    "$renewalDate",
                    renewalDate.ToString("yyyy-MM-dd"));

                updateCommand.Parameters.AddWithValue(
                    "$expiryDate",
                    newExpiryDate.ToString("yyyy-MM-dd"));

                updateCommand.Parameters.AddWithValue(
                    "$lastRenewedByUserId",
                    (object?)lastRenewedByUserId ?? DBNull.Value);

                updateCommand.Parameters.AddWithValue(
                    "$lastRenewedByName",
                    (object?)lastRenewedByName ?? DBNull.Value);

                updateCommand.Parameters.AddWithValue(
                    "$lastRenewedAt",
                    lastRenewedAt.HasValue
                        ? lastRenewedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : (object)DBNull.Value);

                updateCommand.Parameters.AddWithValue(
                    "$previousExpiryDate",
                    previousExpiryDateAudit.HasValue
                        ? previousExpiryDateAudit.Value.ToString("yyyy-MM-dd")
                        : (object)DBNull.Value);

                updateCommand.Parameters.AddWithValue(
                    "$id",
                    qualificationId);

                var rowsAffected = updateCommand.ExecuteNonQuery();

                if (rowsAffected != 1)
                {
                    throw new InvalidOperationException(
                        "The welder qualification could not be updated during renewal.");
                }

                using var historyCommand = connection.CreateCommand();

                historyCommand.Transaction = transaction;

                historyCommand.CommandText = @"
                    INSERT INTO WelderQualificationRenewalHistory
                    (
                        WelderQualificationId,
                        CompanyId,
                        WelderNumber,
                        PreviousExpiryDate,
                        NewExpiryDate,
                        RenewalDate,
                        RenewalPeriodMonths,
                        RenewedByUserId,
                        RenewedByUsername,
                        RenewedByName,
                        Status
                    )
                    VALUES
                    (
                        $qualificationId,
                        $companyId,
                        $welderNumber,
                        $previousExpiryDate,
                        $newExpiryDate,
                        $renewalDate,
                        $renewalPeriodMonths,
                        $renewedByUserId,
                        $renewedByUsername,
                        $renewedByName,
                        'Completed'
                    );
                ";

                historyCommand.Parameters.AddWithValue(
                    "$qualificationId",
                    qualificationId);

                historyCommand.Parameters.AddWithValue(
                    "$companyId",
                    companyId.HasValue
                        ? companyId.Value.ToString()
                        : (object)DBNull.Value);

                historyCommand.Parameters.AddWithValue(
                    "$welderNumber",
                    welderNumber);

                historyCommand.Parameters.AddWithValue(
                    "$previousExpiryDate",
                    previousExpiryDate.ToString("yyyy-MM-dd"));

                historyCommand.Parameters.AddWithValue(
                    "$newExpiryDate",
                    newExpiryDate.ToString("yyyy-MM-dd"));

                historyCommand.Parameters.AddWithValue(
                    "$renewalDate",
                    renewalDate.ToString("yyyy-MM-dd"));

                historyCommand.Parameters.AddWithValue(
                    "$renewalPeriodMonths",
                    renewalPeriodMonths);

                historyCommand.Parameters.AddWithValue(
                    "$renewedByUserId",
                    renewedByUserId);

                historyCommand.Parameters.AddWithValue(
                    "$renewedByUsername",
                    (object?)renewedByUsername ?? DBNull.Value);

                historyCommand.Parameters.AddWithValue(
                    "$renewedByName",
                    renewedByName);

                var historyRowsAffected =
                    historyCommand.ExecuteNonQuery();

                if (historyRowsAffected != 1)
                {
                    throw new InvalidOperationException(
                        "The welder qualification renewal history could not be created.");
                }

                transaction.Commit();
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // Preserve the original exception.
                }

                throw;
            }
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
                    x.IsActive &&
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
                x.IsActive &&
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








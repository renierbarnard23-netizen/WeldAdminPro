using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class WpqrRepository : IWpqrRepository
    {
        private readonly string _databasePath;

        private string _connectionString =>
            $"Data Source={_databasePath}";

        public WpqrRepository()
            : this(DatabasePath.Get())
        {
        }

        public WpqrRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException(
                    "Database path is required.",
                    nameof(databasePath));

            _databasePath = databasePath;
        }

        public void Add(Wpqr wpqr)
        {
            if (wpqr.Id == Guid.Empty)
                wpqr.Id = Guid.NewGuid();

            Validate(wpqr);

            wpqr.WpqrNumber =
                wpqr.WpqrNumber.Trim().ToUpperInvariant();

            if (GetByWpqrNumber(wpqr.WpqrNumber) != null)
            {
                throw new InvalidOperationException(
                    $"WPQR '{wpqr.WpqrNumber}' already exists.");
            }

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO Wpqr
(
    Id,
    CompanyId,
    WpqrNumber,
    WpsId,
    WelderQualificationId,
    QualificationDate,
    RenewalDate,
    ExpiryDate,
    IsActive,
    IsApproved,
    ApprovedBy,
    ApprovedOn,
    ApprovalRole,
    SignaturePath,
    StampPath,
    OriginalDocumentPath,
    PreviousRevisionId,
    Revision,
    IsLocked,
    CreatedOn,
    CreatedBy
)
VALUES
(
    @Id,
    @CompanyId,
    @WpqrNumber,
    @WpsId,
    @WelderQualificationId,
    @QualificationDate,
    @RenewalDate,
    @ExpiryDate,
    @IsActive,
    @IsApproved,
    @ApprovedBy,
    @ApprovedOn,
    @ApprovalRole,
    @SignaturePath,
    @StampPath,
    @OriginalDocumentPath,
    @PreviousRevisionId,
    @Revision,
    @IsLocked,
    @CreatedOn,
    @CreatedBy
);";

            BindParameters(cmd, wpqr);

            cmd.ExecuteNonQuery();
        }

        public void Update(Wpqr wpqr)
        {
            if (wpqr.Id == Guid.Empty)
                throw new InvalidOperationException(
                    "WPQR Id cannot be empty.");

            Validate(wpqr);

            wpqr.WpqrNumber =
                wpqr.WpqrNumber.Trim().ToUpperInvariant();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE Wpqr SET
    CompanyId = @CompanyId,
    WpqrNumber = @WpqrNumber,
    WpsId = @WpsId,
    WelderQualificationId = @WelderQualificationId,
    QualificationDate = @QualificationDate,
    RenewalDate = @RenewalDate,
    ExpiryDate = @ExpiryDate,
    IsActive = @IsActive,
    IsApproved = @IsApproved,
    ApprovedBy = @ApprovedBy,
    ApprovedOn = @ApprovedOn,
    ApprovalRole = @ApprovalRole,
    SignaturePath = @SignaturePath,
    StampPath = @StampPath,
    OriginalDocumentPath = @OriginalDocumentPath,
    PreviousRevisionId = @PreviousRevisionId,
    Revision = @Revision,
    IsLocked = @IsLocked,
    CreatedOn = @CreatedOn,
    CreatedBy = @CreatedBy
WHERE Id = @Id AND IsApproved = 0 AND IsLocked = 0;";

            BindParameters(cmd, wpqr);

            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException(
                    "The WPQR was not updated. It may be missing, approved, or locked. Reload the record and try again.");
            }
        }

        public bool TryApprove(
            Guid id,
            string approvedBy,
            DateTime approvedOn,
            string approvalRole)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("A valid WPQR ID is required.", nameof(id));

            if (string.IsNullOrWhiteSpace(approvedBy))
                throw new ArgumentException("Approver username is required.", nameof(approvedBy));

            if (string.IsNullOrWhiteSpace(approvalRole))
                throw new ArgumentException("Approval role is required.", nameof(approvalRole));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE Wpqr
SET IsApproved = 1,
    IsLocked = 1,
    ApprovedBy = @ApprovedBy,
    ApprovedOn = @ApprovedOn,
    ApprovalRole = @ApprovalRole
WHERE Id = @Id
  AND IsActive = 1
  AND IsApproved = 0
  AND IsLocked = 0;";

            cmd.Parameters.AddWithValue("@Id", id.ToString());
            cmd.Parameters.AddWithValue("@ApprovedBy", approvedBy.Trim());
            cmd.Parameters.AddWithValue("@ApprovedOn", approvedOn.ToUniversalTime().ToString("O"));
            cmd.Parameters.AddWithValue("@ApprovalRole", approvalRole.Trim());

            return cmd.ExecuteNonQuery() == 1;
        }
public void SaveRevision(Wpqr previous, Wpqr revision)
        {
            if (previous == null)
                throw new ArgumentNullException(nameof(previous));

            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            if (previous.Id == Guid.Empty || revision.Id == Guid.Empty)
                throw new InvalidOperationException("Both WPQR revisions must have valid IDs.");

            if (previous.Id == revision.Id)
                throw new InvalidOperationException("A new WPQR revision must have a different ID.");

            if (previous.CompanyId != revision.CompanyId)
                throw new InvalidOperationException("WPQR revisions must belong to the same company.");

            if (!string.Equals(
                    previous.WpqrNumber?.Trim(),
                    revision.WpqrNumber?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("WPQR revisions must retain the same WPQR number.");
            }

            if (revision.PreviousRevisionId != previous.Id)
                throw new InvalidOperationException("The new WPQR revision must reference its predecessor.");

            if (revision.Revision != previous.Revision + 1)
                throw new InvalidOperationException("The new WPQR revision number must increment by one.");

            if (revision.IsApproved || revision.IsLocked || !revision.IsActive)
                throw new InvalidOperationException("A new WPQR revision must be active, unapproved and unlocked.");

            Validate(revision);

            var previousNumber = previous.WpqrNumber?.Trim();
            var normalisedRevisionNumber = revision.WpqrNumber?.Trim();

            if (string.IsNullOrWhiteSpace(previousNumber) ||
                string.IsNullOrWhiteSpace(normalisedRevisionNumber))
            {
                throw new InvalidOperationException(
                    "Both WPQR revision numbers are required.");
            }

            previous.WpqrNumber = previousNumber.ToUpperInvariant();
            revision.WpqrNumber = normalisedRevisionNumber.ToUpperInvariant();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                using (var check = connection.CreateCommand())
                {
                    check.Transaction = transaction;
                    check.CommandText = @"
SELECT IsActive, IsApproved, IsLocked, CompanyId, WpqrNumber, Revision
FROM Wpqr
WHERE Id = @Id
LIMIT 1;";
                    check.Parameters.AddWithValue("@Id", previous.Id.ToString());

                    using var reader = check.ExecuteReader();

                    if (!reader.Read())
                        throw new InvalidOperationException("The previous WPQR revision no longer exists.");

                    var isActive = Convert.ToInt32(reader["IsActive"]) == 1;
                    var isApproved = Convert.ToInt32(reader["IsApproved"]) == 1;
                    var isLocked = Convert.ToInt32(reader["IsLocked"]) == 1;
                    var companyId = reader["CompanyId"] == DBNull.Value
                        ? (Guid?)null
                        : Guid.Parse(reader["CompanyId"].ToString()!);
                    var number = reader["WpqrNumber"]?.ToString() ?? "";
                    var revisionNumber = Convert.ToInt32(reader["Revision"]);

                    if (!isActive)
                        throw new InvalidOperationException("The previous WPQR revision is already inactive.");

                    if (!isApproved && !isLocked)
                        throw new InvalidOperationException("Only an approved or locked WPQR can be revised.");

                    if (companyId != previous.CompanyId ||
                        !string.Equals(number, previous.WpqrNumber, StringComparison.OrdinalIgnoreCase) ||
                        revisionNumber != previous.Revision)
                    {
                        throw new InvalidOperationException("The previous WPQR changed after it was loaded. Reload it and try again.");
                    }
                }

                using (var deactivate = connection.CreateCommand())
                {
                    deactivate.Transaction = transaction;
                    deactivate.CommandText = @"
UPDATE Wpqr
SET IsActive = 0
WHERE Id = @Id
  AND IsActive = 1
  AND (IsApproved = 1 OR IsLocked = 1);";
                    deactivate.Parameters.AddWithValue("@Id", previous.Id.ToString());

                    if (deactivate.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("The previous WPQR revision could not be deactivated.");
                }

                using (var insert = connection.CreateCommand())
                {
                    insert.Transaction = transaction;
                    insert.CommandText = @"
INSERT INTO Wpqr
(
    Id, CompanyId, WpqrNumber, WpsId, WelderQualificationId,
    QualificationDate, RenewalDate, ExpiryDate, IsActive,
    IsApproved, ApprovedBy, ApprovedOn, ApprovalRole,
    SignaturePath, StampPath, OriginalDocumentPath,
    PreviousRevisionId, Revision, IsLocked, CreatedOn, CreatedBy
)
VALUES
(
    @Id, @CompanyId, @WpqrNumber, @WpsId, @WelderQualificationId,
    @QualificationDate, @RenewalDate, @ExpiryDate, @IsActive,
    @IsApproved, @ApprovedBy, @ApprovedOn, @ApprovalRole,
    @SignaturePath, @StampPath, @OriginalDocumentPath,
    @PreviousRevisionId, @Revision, @IsLocked, @CreatedOn, @CreatedBy
);";
                    BindParameters(insert, revision);
                    insert.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                try { transaction.Rollback(); }
                catch { /* Preserve the original exception. */ }
                throw;
            }
        }
        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("A valid WPQR ID is required.", nameof(id));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
DELETE FROM Wpqr
WHERE Id = @Id
  AND IsApproved = 0
  AND IsLocked = 0;
";

            cmd.Parameters.AddWithValue(
                "@Id",
                id.ToString());

            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException(
                    "The WPQR was not deleted. It may be missing, approved, or locked.");
            }
        }
        public List<Wpqr> GetAll()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
ORDER BY QualificationDate DESC, WpqrNumber;";

            using var reader = cmd.ExecuteReader();

            var list = new List<Wpqr>();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public Wpqr? GetById(Guid id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE Id = @Id
LIMIT 1;";

            cmd.Parameters.AddWithValue(
                "@Id",
                id.ToString());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        public Wpqr? GetByWpqrNumber(string wpqrNumber)
        {
            if (string.IsNullOrWhiteSpace(wpqrNumber))
                return null;

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE WpqrNumber = @WpqrNumber
LIMIT 1;";

            cmd.Parameters.AddWithValue(
                "@WpqrNumber",
                wpqrNumber.Trim().ToUpperInvariant());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        public List<Wpqr> GetByWpsId(Guid wpsId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE WpsId = @WpsId
ORDER BY QualificationDate DESC;";

            cmd.Parameters.AddWithValue(
                "@WpsId",
                wpsId.ToString());

            using var reader = cmd.ExecuteReader();

            var list = new List<Wpqr>();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Wpqr> GetByWelderQualificationId(
            int welderQualificationId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE WelderQualificationId = @WelderQualificationId
ORDER BY QualificationDate DESC;";

            cmd.Parameters.AddWithValue(
                "@WelderQualificationId",
                welderQualificationId);

            using var reader = cmd.ExecuteReader();

            var list = new List<Wpqr>();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Wpqr> GetActive()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE IsActive = 1
ORDER BY ExpiryDate, WpqrNumber;";

            using var reader = cmd.ExecuteReader();

            var list = new List<Wpqr>();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Wpqr> GetApproved()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wpqr
WHERE IsApproved = 1
ORDER BY ExpiryDate, WpqrNumber;";

            using var reader = cmd.ExecuteReader();

            var list = new List<Wpqr>();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        private static void Validate(Wpqr wpqr)
        {
            if (string.IsNullOrWhiteSpace(wpqr.WpqrNumber))
                throw new InvalidOperationException(
                    "WPQR Number is required.");

            if (!wpqr.WpsId.HasValue)
                throw new InvalidOperationException(
                    "WPS reference is required.");

            if (!wpqr.WelderQualificationId.HasValue)
                throw new InvalidOperationException(
                    "Welder qualification is required.");
        }

        private static void BindParameters(
            SqliteCommand cmd,
            Wpqr wpqr)
        {
            cmd.Parameters.AddWithValue(
                "@Id",
                wpqr.Id.ToString());

            cmd.Parameters.AddWithValue(
                "@CompanyId",
                wpqr.CompanyId?.ToString() ??
                (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@WpqrNumber",
                wpqr.WpqrNumber);

            cmd.Parameters.AddWithValue(
                "@WpsId",
                wpqr.WpsId?.ToString() ??
                (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@WelderQualificationId",
                wpqr.WelderQualificationId.HasValue
                    ? wpqr.WelderQualificationId.Value
                    : (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@QualificationDate",
                wpqr.QualificationDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue(
                "@RenewalDate",
                wpqr.RenewalDate?.ToString("yyyy-MM-dd")
                    ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@ExpiryDate",
                wpqr.ExpiryDate.ToString("yyyy-MM-dd"));

            cmd.Parameters.AddWithValue(
                "@IsActive",
                wpqr.IsActive ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "@IsApproved",
                wpqr.IsApproved ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "@ApprovedBy",
                (object?)wpqr.ApprovedBy ??
                DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@ApprovedOn",
                wpqr.ApprovedOn?.ToString("s")
                    ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@ApprovalRole",
                (object?)wpqr.ApprovalRole ??
                DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@SignaturePath",
                (object?)wpqr.SignaturePath ??
                DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@StampPath",
                (object?)wpqr.StampPath ??
                DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@OriginalDocumentPath",
                (object?)wpqr.OriginalDocumentPath ??
                DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@PreviousRevisionId",
                wpqr.PreviousRevisionId?.ToString() ??
                (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Revision",
                wpqr.Revision);

            cmd.Parameters.AddWithValue(
                "@IsLocked",
                wpqr.IsLocked ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "@CreatedOn",
                wpqr.CreatedOn.ToString("O"));

            cmd.Parameters.AddWithValue(
                "@CreatedBy",
                wpqr.CreatedBy);
        }

        private static Wpqr Map(
            SqliteDataReader reader)
        {
            return new Wpqr
            {
                Id = Guid.Parse(
                    reader["Id"].ToString()!),

                CompanyId =
                    reader["CompanyId"] == DBNull.Value
                        ? null
                        : Guid.Parse(reader["CompanyId"].ToString()!),

                WpqrNumber =
                    reader["WpqrNumber"]?.ToString()
                    ?? "",

                WpsId =
                    reader["WpsId"] == DBNull.Value
                        ? null
                        : Guid.Parse(
                            reader["WpsId"].ToString()!),

                WelderQualificationId =
                    reader["WelderQualificationId"]
                        == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["WelderQualificationId"]),

                QualificationDate =
                    DateTime.Parse(
                        reader["QualificationDate"]
                            .ToString()!),

                RenewalDate =
                    reader["RenewalDate"]
                        == DBNull.Value
                        ? null
                        : DateTime.Parse(
                            reader["RenewalDate"]
                                .ToString()!),

                ExpiryDate =
                    DateTime.Parse(
                        reader["ExpiryDate"]
                            .ToString()!),

                IsActive =
                    Convert.ToInt32(
                        reader["IsActive"]) == 1,

                IsApproved =
                    Convert.ToInt32(
                        reader["IsApproved"]) == 1,

                ApprovedBy =
                    reader["ApprovedBy"]
                        == DBNull.Value
                        ? null
                        : reader["ApprovedBy"].ToString(),

                ApprovedOn =
                    reader["ApprovedOn"]
                        == DBNull.Value
                        ? null
                        : DateTime.Parse(
                            reader["ApprovedOn"].ToString()!),

                ApprovalRole =
                    reader["ApprovalRole"]
                        == DBNull.Value
                        ? null
                        : reader["ApprovalRole"].ToString(),

                SignaturePath =
                    reader["SignaturePath"]
                        == DBNull.Value
                        ? null
                        : reader["SignaturePath"].ToString(),

                StampPath =
                    reader["StampPath"]
                        == DBNull.Value
                        ? null
                        : reader["StampPath"].ToString(),

                OriginalDocumentPath =
                    reader["OriginalDocumentPath"]
                        == DBNull.Value
                        ? null
                        : reader["OriginalDocumentPath"].ToString(),

                PreviousRevisionId =
                    reader["PreviousRevisionId"] == DBNull.Value
                        ? null
                        : Guid.Parse(reader["PreviousRevisionId"].ToString()!),

                Revision = Convert.ToInt32(reader["Revision"]),

                IsLocked = Convert.ToInt32(reader["IsLocked"]) == 1,

                CreatedOn =
                    DateTime.Parse(
                        reader["CreatedOn"].ToString()!,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.RoundtripKind),

                CreatedBy =
                    reader["CreatedBy"]?.ToString()
                    ?? ""
            };
        }
    }
}






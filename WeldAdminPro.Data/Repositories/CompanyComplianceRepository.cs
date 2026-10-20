using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class CompanyComplianceRepository
    {
        private readonly string _connectionString;

        public CompanyComplianceRepository()
        {
            _connectionString =
                $"Data Source={DatabasePath.Get()}";
        }

        // ==================================================
        // CONTACTS
        // ==================================================

        public List<CompanyContact> GetContacts(Guid companyId)
        {
            var list = new List<CompanyContact>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    Id,
                    CompanyId,
                    ContactName,
                    Position,
                    Telephone,
                    Mobile,
                    Email,
                    IsPrimary,
                    IsActive,
                    CreatedDate
                FROM CompanyContacts
                WHERE CompanyId = $companyId
                ORDER BY IsPrimary DESC, ContactName;
            ";

            cmd.Parameters.AddWithValue(
                "$companyId",
                companyId.ToString());

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapContact(reader));
            }

            return list;
        }

        public CompanyContact? GetPrimaryContact(
            Guid companyId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    Id,
                    CompanyId,
                    ContactName,
                    Position,
                    Telephone,
                    Mobile,
                    Email,
                    IsPrimary,
                    IsActive,
                    CreatedDate
                FROM CompanyContacts
                WHERE CompanyId = $companyId
                  AND IsPrimary = 1
                  AND IsActive = 1
                ORDER BY ContactName
                LIMIT 1;
            ";

            cmd.Parameters.AddWithValue(
                "$companyId",
                companyId.ToString());

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapContact(reader);
        }

        public void AddContact(
            CompanyContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(
                    nameof(contact));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                if (contact.IsPrimary)
                {
                    using var clearPrimary =
                        connection.CreateCommand();

                    clearPrimary.Transaction =
                        transaction;

                    clearPrimary.CommandText = @"
                        UPDATE CompanyContacts
                        SET IsPrimary = 0
                        WHERE CompanyId = $companyId;
                    ";

                    clearPrimary.Parameters.AddWithValue(
                        "$companyId",
                        contact.CompanyId.ToString());

                    clearPrimary.ExecuteNonQuery();
                }

                using var cmd =
                    connection.CreateCommand();

                cmd.Transaction = transaction;

                cmd.CommandText = @"
                    INSERT INTO CompanyContacts
                    (
                        Id,
                        CompanyId,
                        ContactName,
                        Position,
                        Telephone,
                        Mobile,
                        Email,
                        IsPrimary,
                        IsActive,
                        CreatedDate
                    )
                    VALUES
                    (
                        $id,
                        $companyId,
                        $contactName,
                        $position,
                        $telephone,
                        $mobile,
                        $email,
                        $isPrimary,
                        $isActive,
                        $createdDate
                    );
                ";

                AddContactParameters(cmd, contact);

                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateContact(
            CompanyContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(
                    nameof(contact));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                if (contact.IsPrimary)
                {
                    using var clearPrimary =
                        connection.CreateCommand();

                    clearPrimary.Transaction =
                        transaction;

                    clearPrimary.CommandText = @"
                        UPDATE CompanyContacts
                        SET IsPrimary = 0
                        WHERE CompanyId = $companyId
                          AND Id <> $id;
                    ";

                    clearPrimary.Parameters.AddWithValue(
                        "$companyId",
                        contact.CompanyId.ToString());

                    clearPrimary.Parameters.AddWithValue(
                        "$id",
                        contact.Id.ToString());

                    clearPrimary.ExecuteNonQuery();
                }

                using var cmd =
                    connection.CreateCommand();

                cmd.Transaction = transaction;

                cmd.CommandText = @"
                    UPDATE CompanyContacts
                    SET
                        ContactName = $contactName,
                        Position = $position,
                        Telephone = $telephone,
                        Mobile = $mobile,
                        Email = $email,
                        IsPrimary = $isPrimary,
                        IsActive = $isActive
                    WHERE Id = $id
                      AND CompanyId = $companyId;
                ";

                cmd.Parameters.AddWithValue(
                    "$contactName",
                    contact.ContactName?.Trim() ?? "");

                cmd.Parameters.AddWithValue(
                    "$position",
                    string.IsNullOrWhiteSpace(contact.Position)
                        ? DBNull.Value
                        : contact.Position.Trim());

                cmd.Parameters.AddWithValue(
                    "$telephone",
                    string.IsNullOrWhiteSpace(contact.Telephone)
                        ? DBNull.Value
                        : contact.Telephone.Trim());

                cmd.Parameters.AddWithValue(
                    "$mobile",
                    string.IsNullOrWhiteSpace(contact.Mobile)
                        ? DBNull.Value
                        : contact.Mobile.Trim());

                cmd.Parameters.AddWithValue(
                    "$email",
                    string.IsNullOrWhiteSpace(contact.Email)
                        ? DBNull.Value
                        : contact.Email.Trim());

                cmd.Parameters.AddWithValue(
                    "$isPrimary",
                    contact.IsPrimary ? 1 : 0);

                cmd.Parameters.AddWithValue(
                    "$isActive",
                    contact.IsActive ? 1 : 0);

                cmd.Parameters.AddWithValue(
                    "$id",
                    contact.Id.ToString());

                cmd.Parameters.AddWithValue(
                    "$companyId",
                    contact.CompanyId.ToString());

                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SetContactActive(
            Guid id,
            bool isActive)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                UPDATE CompanyContacts
                SET IsActive = $active
                WHERE Id = $id;
            ";

            cmd.Parameters.AddWithValue(
                "$active",
                isActive ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$id",
                id.ToString());

            cmd.ExecuteNonQuery();
        }

        // ==================================================
        // REQUIREMENTS
        // ==================================================

        public List<CompanyRequirement> GetRequirements(
            Guid companyId)
        {
            var list = new List<CompanyRequirement>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    Id,
                    CompanyId,
                    RequirementCode,
                    RequirementName,
                    RequirementType,
                    RequirementStatus,
                    Notes,
                    ReviewedDate,
                    ReviewedBy,
                    CreatedDate
                FROM CompanyRequirements
                WHERE CompanyId = $companyId
                ORDER BY RequirementType, RequirementName;
            ";

            cmd.Parameters.AddWithValue(
                "$companyId",
                companyId.ToString());

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapRequirement(reader));
            }

            return list;
        }

        public void AddRequirement(
            CompanyRequirement requirement)
        {
            if (requirement == null)
                throw new ArgumentNullException(
                    nameof(requirement));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO CompanyRequirements
                (
                    Id,
                    CompanyId,
                    RequirementCode,
                    RequirementName,
                    RequirementType,
                    RequirementStatus,
                    Notes,
                    ReviewedDate,
                    ReviewedBy,
                    CreatedDate
                )
                VALUES
                (
                    $id,
                    $companyId,
                    $requirementCode,
                    $requirementName,
                    $requirementType,
                    $requirementStatus,
                    $notes,
                    $reviewedDate,
                    $reviewedBy,
                    $createdDate
                );
            ";

            AddRequirementParameters(
                cmd,
                requirement);

            cmd.ExecuteNonQuery();
        }

        public void UpdateRequirement(
            CompanyRequirement requirement)
        {
            if (requirement == null)
                throw new ArgumentNullException(
                    nameof(requirement));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                UPDATE CompanyRequirements
                SET
                    RequirementCode = $requirementCode,
                    RequirementName = $requirementName,
                    RequirementType = $requirementType,
                    RequirementStatus = $requirementStatus,
                    Notes = $notes,
                    ReviewedDate = $reviewedDate,
                    ReviewedBy = $reviewedBy
                WHERE Id = $id
                  AND CompanyId = $companyId;
            ";

            cmd.Parameters.AddWithValue(
                "$requirementCode",
                requirement.RequirementCode?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementName",
                requirement.RequirementName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementType",
                requirement.RequirementType?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementStatus",
                requirement.RequirementStatus?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$notes",
                string.IsNullOrWhiteSpace(requirement.Notes)
                    ? DBNull.Value
                    : requirement.Notes.Trim());

            cmd.Parameters.AddWithValue(
                "$reviewedDate",
                requirement.ReviewedDate.HasValue
                    ? requirement.ReviewedDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$reviewedBy",
                string.IsNullOrWhiteSpace(requirement.ReviewedBy)
                    ? DBNull.Value
                    : requirement.ReviewedBy.Trim());

            cmd.Parameters.AddWithValue(
                "$id",
                requirement.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$companyId",
                requirement.CompanyId.ToString());

            cmd.ExecuteNonQuery();
        }

        // ==================================================
        // DOCUMENTS
        // ==================================================

        public List<CompanyDocument> GetDocuments(
            Guid companyId)
        {
            var list = new List<CompanyDocument>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    Id,
                    CompanyId,
                    DocumentType,
                    DocumentName,
                    FileName,
                    FilePath,
                    IssueDate,
                    ExpiryDate,
                    ReviewDate,
                    RequiresRenewal,
                    ReminderDays,
                    IsNA,
                    NAReason,
                    UploadedDate,
                    UploadedBy,
                    IsActive
                FROM CompanyDocuments
                WHERE CompanyId = $companyId
                ORDER BY DocumentType, DocumentName;
            ";

            cmd.Parameters.AddWithValue(
                "$companyId",
                companyId.ToString());

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapDocument(reader));
            }

            return list;
        }

        public List<CompanyDocument> GetDocumentsRequiringAttention(
            Guid companyId,
            DateTime today)
        {
            var list = new List<CompanyDocument>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    Id,
                    CompanyId,
                    DocumentType,
                    DocumentName,
                    FileName,
                    FilePath,
                    IssueDate,
                    ExpiryDate,
                    ReviewDate,
                    RequiresRenewal,
                    ReminderDays,
                    IsNA,
                    NAReason,
                    UploadedDate,
                    UploadedBy,
                    IsActive
                FROM CompanyDocuments
                WHERE CompanyId = $companyId
                  AND IsActive = 1
                  AND IsNA = 0
                  AND
                  (
                      (
                          RequiresRenewal = 1
                          AND ExpiryDate IS NOT NULL
                          AND date(ExpiryDate)
                              <= date($attentionDate)
                      )
                      OR
                      (
                          ReviewDate IS NOT NULL
                          AND date(ReviewDate)
                              <= date($attentionDate)
                      )
                  )
                ORDER BY
                    COALESCE(ExpiryDate, ReviewDate);
            ";

            cmd.Parameters.AddWithValue(
                "$companyId",
                companyId.ToString());

            cmd.Parameters.AddWithValue(
                "$attentionDate",
                today.ToString("yyyy-MM-dd"));

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapDocument(reader));
            }

            return list;
        }

        public void AddDocument(
            CompanyDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(
                    nameof(document));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO CompanyDocuments
                (
                    Id,
                    CompanyId,
                    DocumentType,
                    DocumentName,
                    FileName,
                    FilePath,
                    IssueDate,
                    ExpiryDate,
                    ReviewDate,
                    RequiresRenewal,
                    ReminderDays,
                    IsNA,
                    NAReason,
                    UploadedDate,
                    UploadedBy,
                    IsActive
                )
                VALUES
                (
                    $id,
                    $companyId,
                    $documentType,
                    $documentName,
                    $fileName,
                    $filePath,
                    $issueDate,
                    $expiryDate,
                    $reviewDate,
                    $requiresRenewal,
                    $reminderDays,
                    $isNA,
                    $naReason,
                    $uploadedDate,
                    $uploadedBy,
                    $isActive
                );
            ";

            AddDocumentParameters(
                cmd,
                document);

            cmd.ExecuteNonQuery();
        }

        public void UpdateDocument(
            CompanyDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(
                    nameof(document));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                UPDATE CompanyDocuments
                SET
                    DocumentType = $documentType,
                    DocumentName = $documentName,
                    FileName = $fileName,
                    FilePath = $filePath,
                    IssueDate = $issueDate,
                    ExpiryDate = $expiryDate,
                    ReviewDate = $reviewDate,
                    RequiresRenewal = $requiresRenewal,
                    ReminderDays = $reminderDays,
                    IsNA = $isNA,
                    NAReason = $naReason,
                    UploadedBy = $uploadedBy,
                    IsActive = $isActive
                WHERE Id = $id
                  AND CompanyId = $companyId;
            ";

            cmd.Parameters.AddWithValue(
                "$documentType",
                document.DocumentType?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$documentName",
                document.DocumentName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$fileName",
                document.FileName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$filePath",
                document.FilePath?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$issueDate",
                document.IssueDate.HasValue
                    ? document.IssueDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$expiryDate",
                document.ExpiryDate.HasValue
                    ? document.ExpiryDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$reviewDate",
                document.ReviewDate.HasValue
                    ? document.ReviewDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$requiresRenewal",
                document.RequiresRenewal ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$reminderDays",
                document.ReminderDays);

            cmd.Parameters.AddWithValue(
                "$isNA",
                document.IsNA ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$naReason",
                string.IsNullOrWhiteSpace(document.NAReason)
                    ? DBNull.Value
                    : document.NAReason.Trim());

            cmd.Parameters.AddWithValue(
                "$uploadedBy",
                string.IsNullOrWhiteSpace(document.UploadedBy)
                    ? DBNull.Value
                    : document.UploadedBy.Trim());

            cmd.Parameters.AddWithValue(
                "$isActive",
                document.IsActive ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$id",
                document.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$companyId",
                document.CompanyId.ToString());

            cmd.ExecuteNonQuery();
        }

        public void SetDocumentActive(
            Guid id,
            bool isActive)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                UPDATE CompanyDocuments
                SET IsActive = $active
                WHERE Id = $id;
            ";

            cmd.Parameters.AddWithValue(
                "$active",
                isActive ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$id",
                id.ToString());

            cmd.ExecuteNonQuery();
        }

        // ==================================================
        // PARAMETER HELPERS
        // ==================================================

        private static void AddContactParameters(
            SqliteCommand cmd,
            CompanyContact contact)
        {
            cmd.Parameters.AddWithValue(
                "$id",
                contact.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$companyId",
                contact.CompanyId.ToString());

            cmd.Parameters.AddWithValue(
                "$contactName",
                contact.ContactName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$position",
                string.IsNullOrWhiteSpace(contact.Position)
                    ? DBNull.Value
                    : contact.Position.Trim());

            cmd.Parameters.AddWithValue(
                "$telephone",
                string.IsNullOrWhiteSpace(contact.Telephone)
                    ? DBNull.Value
                    : contact.Telephone.Trim());

            cmd.Parameters.AddWithValue(
                "$mobile",
                string.IsNullOrWhiteSpace(contact.Mobile)
                    ? DBNull.Value
                    : contact.Mobile.Trim());

            cmd.Parameters.AddWithValue(
                "$email",
                string.IsNullOrWhiteSpace(contact.Email)
                    ? DBNull.Value
                    : contact.Email.Trim());

            cmd.Parameters.AddWithValue(
                "$isPrimary",
                contact.IsPrimary ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$isActive",
                contact.IsActive ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$createdDate",
                contact.CreatedDate.ToString("O"));
        }

        private static void AddRequirementParameters(
            SqliteCommand cmd,
            CompanyRequirement requirement)
        {
            cmd.Parameters.AddWithValue(
                "$id",
                requirement.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$companyId",
                requirement.CompanyId.ToString());

            cmd.Parameters.AddWithValue(
                "$requirementCode",
                requirement.RequirementCode?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementName",
                requirement.RequirementName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementType",
                requirement.RequirementType?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$requirementStatus",
                requirement.RequirementStatus?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$notes",
                string.IsNullOrWhiteSpace(requirement.Notes)
                    ? DBNull.Value
                    : requirement.Notes.Trim());

            cmd.Parameters.AddWithValue(
                "$reviewedDate",
                requirement.ReviewedDate.HasValue
                    ? requirement.ReviewedDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$reviewedBy",
                string.IsNullOrWhiteSpace(requirement.ReviewedBy)
                    ? DBNull.Value
                    : requirement.ReviewedBy.Trim());

            cmd.Parameters.AddWithValue(
                "$createdDate",
                requirement.CreatedDate.ToString("O"));
        }

        private static void AddDocumentParameters(
            SqliteCommand cmd,
            CompanyDocument document)
        {
            cmd.Parameters.AddWithValue(
                "$id",
                document.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$companyId",
                document.CompanyId.ToString());

            cmd.Parameters.AddWithValue(
                "$documentType",
                document.DocumentType?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$documentName",
                document.DocumentName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$fileName",
                document.FileName?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$filePath",
                document.FilePath?.Trim() ?? "");

            cmd.Parameters.AddWithValue(
                "$issueDate",
                document.IssueDate.HasValue
                    ? document.IssueDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$expiryDate",
                document.ExpiryDate.HasValue
                    ? document.ExpiryDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$reviewDate",
                document.ReviewDate.HasValue
                    ? document.ReviewDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$requiresRenewal",
                document.RequiresRenewal ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$reminderDays",
                document.ReminderDays);

            cmd.Parameters.AddWithValue(
                "$isNA",
                document.IsNA ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$naReason",
                string.IsNullOrWhiteSpace(document.NAReason)
                    ? DBNull.Value
                    : document.NAReason.Trim());

            cmd.Parameters.AddWithValue(
                "$uploadedDate",
                document.UploadedDate.ToString("O"));

            cmd.Parameters.AddWithValue(
                "$uploadedBy",
                string.IsNullOrWhiteSpace(document.UploadedBy)
                    ? DBNull.Value
                    : document.UploadedBy.Trim());

            cmd.Parameters.AddWithValue(
                "$isActive",
                document.IsActive ? 1 : 0);
        }

        // ==================================================
        // MAPPERS
        // ==================================================

        private static CompanyContact MapContact(
            SqliteDataReader reader)
        {
            return new CompanyContact
            {
                Id = Guid.Parse(reader.GetString(0)),

                CompanyId =
                    Guid.Parse(reader.GetString(1)),

                ContactName =
                    reader.GetString(2),

                Position =
                    GetNullableString(reader, 3),

                Telephone =
                    GetNullableString(reader, 4),

                Mobile =
                    GetNullableString(reader, 5),

                Email =
                    GetNullableString(reader, 6),

                IsPrimary =
                    reader.GetInt32(7) == 1,

                IsActive =
                    reader.GetInt32(8) == 1,

                CreatedDate =
                    DateTime.Parse(reader.GetString(9))
            };
        }

        private static CompanyRequirement MapRequirement(
            SqliteDataReader reader)
        {
            return new CompanyRequirement
            {
                Id = Guid.Parse(reader.GetString(0)),

                CompanyId =
                    Guid.Parse(reader.GetString(1)),

                RequirementCode =
                    reader.GetString(2),

                RequirementName =
                    reader.GetString(3),

                RequirementType =
                    reader.GetString(4),

                RequirementStatus =
                    reader.GetString(5),

                Notes =
                    GetNullableString(reader, 6),

                ReviewedDate =
                    GetNullableDateTime(reader, 7),

                ReviewedBy =
                    GetNullableString(reader, 8),

                CreatedDate =
                    DateTime.Parse(reader.GetString(9))
            };
        }

        private static CompanyDocument MapDocument(
            SqliteDataReader reader)
        {
            return new CompanyDocument
            {
                Id = Guid.Parse(reader.GetString(0)),

                CompanyId =
                    Guid.Parse(reader.GetString(1)),

                DocumentType =
                    reader.GetString(2),

                DocumentName =
                    reader.GetString(3),

                FileName =
                    reader.GetString(4),

                FilePath =
                    reader.GetString(5),

                IssueDate =
                    GetNullableDateTime(reader, 6),

                ExpiryDate =
                    GetNullableDateTime(reader, 7),

                ReviewDate =
                    GetNullableDateTime(reader, 8),

                RequiresRenewal =
                    reader.GetInt32(9) == 1,

                ReminderDays =
                    reader.GetInt32(10),

                IsNA =
                    reader.GetInt32(11) == 1,

                NAReason =
                    GetNullableString(reader, 12),

                UploadedDate =
                    DateTime.Parse(reader.GetString(13)),

                UploadedBy =
                    GetNullableString(reader, 14),

                IsActive =
                    reader.GetInt32(15) == 1
            };
        }

        private static string GetNullableString(
            SqliteDataReader reader,
            int ordinal)
        {
            return reader.IsDBNull(ordinal)
                ? ""
                : reader.GetString(ordinal);
        }

        private static DateTime? GetNullableDateTime(
            SqliteDataReader reader,
            int ordinal)
        {
            return reader.IsDBNull(ordinal)
                ? null
                : DateTime.Parse(reader.GetString(ordinal));
        }
    }
}
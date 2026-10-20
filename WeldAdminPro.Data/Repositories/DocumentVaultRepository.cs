using Microsoft.Data.Sqlite;
using System;
using System.IO;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Interfaces;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class DocumentVaultRepository
        : IDocumentVaultRepository
    {
        private readonly string _connectionString;

        public DocumentVaultRepository(
        string connectionString)
        {
            _connectionString =
                connectionString;

            CreateIndexes();
        }

        public DocumentVaultFile? GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT *
FROM DocumentVaultFiles
WHERE Id = $Id
LIMIT 1;";
            command.Parameters.AddWithValue("$Id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new DocumentVaultFile
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                FileName = reader["FileName"]?.ToString() ?? "",
                OriginalFileName = reader["OriginalFileName"]?.ToString() ?? "",
                FilePath = reader["FilePath"]?.ToString() ?? "",
                Category = (DocumentCategoryType)Convert.ToInt32(reader["Category"]),
                Description = reader["Description"]?.ToString() ?? "",
                DocumentNumber = reader["DocumentNumber"]?.ToString() ?? "",
                Title = reader["Title"]?.ToString() ?? "",
                Status = reader["Status"]?.ToString() ?? "",
                Revision = reader["Revision"]?.ToString() ?? "",
                UploadedDate = DateTime.Parse(reader["UploadedDate"].ToString()!),
                UploadedBy = reader["UploadedBy"]?.ToString() ?? "",
                WeldId = reader["WeldId"] == DBNull.Value ? null : Guid.Parse(reader["WeldId"].ToString()!),
                ProjectId = reader["ProjectId"] == DBNull.Value ? null : Guid.Parse(reader["ProjectId"].ToString()!),
                CompanyId = reader["CompanyId"] == DBNull.Value ? null : Guid.Parse(reader["CompanyId"].ToString()!),
                IsApproved = Convert.ToInt32(reader["IsApproved"]) == 1
            };
        }

        public void Add(
            DocumentVaultFile file)
        {
            if (file.Id == Guid.Empty)
            {
                file.Id = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                throw new InvalidOperationException(
                    "File name is required.");
            }

            if (string.IsNullOrWhiteSpace(file.FilePath))
            {
                throw new InvalidOperationException(
                    "File path is required.");
            }

            if (!File.Exists(file.FilePath))
            {
                throw new FileNotFoundException(
                    $"Document not found: {file.FilePath}");
            }

            if (file.UploadedDate == default)
            {
                file.UploadedDate = DateTime.UtcNow;
            }

            var normalizedName = file.FileName
                .Trim()
                .ToUpperInvariant();

            file.FileName =
                normalizedName;

            if (Exists(
                    file.ProjectId,
                    file.FileName,
                    file.CompanyId))
            {
                throw new InvalidOperationException(
                    $"Document '{file.FileName}' already exists.");
            }

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
INSERT INTO DocumentVaultFiles
(
    Id,
    FileName,
    OriginalFileName,
    FilePath,
    Category,
    Description,
    DocumentNumber,
    Title,
    Status,
    Revision,
    UploadedDate,
    UploadedBy,
    WeldId,
    ProjectId,
    CompanyId,
    IsApproved
)
VALUES
(
    $Id,
    $FileName,
    $OriginalFileName,
    $FilePath,
    $Category,
    $Description,
    $DocumentNumber, 
    $Title, 
    $Status,
    $Revision,
    $UploadedDate,
    $UploadedBy,
    $WeldId,
    $ProjectId,
    $CompanyId,
    $IsApproved
)";

            command.Parameters.AddWithValue(
                "$Id",
                file.Id.ToString());

            command.Parameters.AddWithValue(
                "$FileName",
                file.FileName);

            command.Parameters.AddWithValue(
                "$OriginalFileName",
                file.OriginalFileName);

            command.Parameters.AddWithValue(
                "$FilePath",
                file.FilePath);

            command.Parameters.AddWithValue(
                "$Category",
                (int)file.Category);

            command.Parameters.AddWithValue(
                "$Description",
                file.Description);

            command.Parameters.AddWithValue(
                "$DocumentNumber",
                file.DocumentNumber ?? "");

            command.Parameters.AddWithValue(
                "$Title",
                file.Title ?? "");

            command.Parameters.AddWithValue(
                "$Status",
                file.Status ?? "");

            command.Parameters.AddWithValue(
                "$Revision",
                file.Revision);

            command.Parameters.AddWithValue(
                "$UploadedDate",
                file.UploadedDate.ToString("O"));

            command.Parameters.AddWithValue(
                "$UploadedBy",
                file.UploadedBy);

            command.Parameters.AddWithValue(
                "$WeldId",
                file.WeldId != null
                    ? file.WeldId.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "$ProjectId",
                file.ProjectId != null
                    ? file.ProjectId.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "$CompanyId",
                file.CompanyId != null
                    ? file.CompanyId.ToString()
                    : DBNull.Value);


            command.Parameters.AddWithValue(
                "$IsApproved",
                file.IsApproved ? 1 : 0);

            command.ExecuteNonQuery();
        }

        public void Approve(
            Guid id,
            string approvedBy)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Document ID is required.",
                    nameof(id));

            if (string.IsNullOrWhiteSpace(approvedBy))
                throw new ArgumentException(
                    "Approving user is required.",
                    nameof(approvedBy));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            using var check =
                connection.CreateCommand();

            check.Transaction = transaction;
            check.CommandText = @"
SELECT IsApproved
FROM DocumentVaultFiles
WHERE Id = $Id
LIMIT 1;";

            check.Parameters.AddWithValue(
                "$Id",
                id.ToString());

            var existing = check.ExecuteScalar();

            if (existing == null ||
                existing == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Document could not be found.");
            }

            if (Convert.ToInt32(existing) == 1)
            {
                throw new InvalidOperationException(
                    "Document is already approved.");
            }

            using var command =
                connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
UPDATE DocumentVaultFiles
SET
    IsApproved = 1,
    Status = 'Approved'
WHERE Id = $Id
  AND IsApproved = 0;";

            command.Parameters.AddWithValue(
                "$Id",
                id.ToString());

            var affected =
                command.ExecuteNonQuery();

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "Document approval could not be completed.");
            }

            transaction.Commit();
        }
        private void CreateIndexes()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
CREATE INDEX IF NOT EXISTS
IX_DocumentVault_ProjectId
ON DocumentVaultFiles(ProjectId);

CREATE INDEX IF NOT EXISTS
IX_DocumentVault_WeldId
ON DocumentVaultFiles(WeldId);

CREATE INDEX IF NOT EXISTS
IX_DocumentVault_Category
ON DocumentVaultFiles(Category);

CREATE INDEX IF NOT EXISTS
IX_DocumentVault_UploadedDate
ON DocumentVaultFiles(UploadedDate);

CREATE INDEX IF NOT EXISTS
IX_DocumentVault_IsApproved
ON DocumentVaultFiles(IsApproved);
";

            cmd.ExecuteNonQuery();
        }

        public List<DocumentVaultFile> GetAll()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
                "SELECT * FROM DocumentVaultFiles ORDER BY UploadedDate DESC";

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }

        public List<DocumentVaultFile>
            GetByProject(Guid projectId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
                SELECT *
                FROM DocumentVaultFiles
                WHERE ProjectId = $ProjectId
                ORDER BY UploadedDate DESC;";

            command.Parameters.AddWithValue(
                "$ProjectId",
                projectId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }

        public List<DocumentVaultFile>
            GetByCompany(Guid companyId)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException(
                    "Company ID is required.",
                    nameof(companyId));

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
                SELECT *
                FROM DocumentVaultFiles
                WHERE CompanyId = $CompanyId
                ORDER BY UploadedDate DESC;";

            command.Parameters.AddWithValue(
                "$CompanyId",
                companyId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }

        public List<DocumentVaultFile>
            GetByWeld(Guid weldId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
                SELECT *
                FROM DocumentVaultFiles
                WHERE WeldId = $WeldId
                ORDER BY UploadedDate DESC;";

            command.Parameters.AddWithValue(
                "$WeldId",
                weldId.ToString());

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }

        public List<DocumentVaultFile>
            GetApprovedByCompanyAndCategory(
            Guid companyId,
            DocumentCategoryType category)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException(
                    "Company ID is required.",
                    nameof(companyId));

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
                SELECT *
                FROM DocumentVaultFiles
                WHERE CompanyId = $CompanyId
                AND Category = $Category
                AND IsApproved = 1
                ORDER BY UploadedDate DESC;";

            command.Parameters.AddWithValue(
                "$CompanyId",
                companyId.ToString());

            command.Parameters.AddWithValue(
                "$Category",
                (int)category);

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }

        public List<DocumentVaultFile>
            GetApprovedByCategory(
            DocumentCategoryType category)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText =
            @"
SELECT *
FROM DocumentVaultFiles
WHERE Category = $Category
AND IsApproved = 1
ORDER BY UploadedDate DESC";

            command.Parameters.AddWithValue(
                "$Category",
                (int)category);

            using var reader =
                command.ExecuteReader();

            return ReadFiles(reader);
        }


        private List<DocumentVaultFile>
            ReadFiles(SqliteDataReader reader)
        {

            var files =
                new List<DocumentVaultFile>();

            while (reader.Read())
            {
                files.Add(
                    new DocumentVaultFile
                    {
                        Id =
                            Guid.Parse(
                                reader.GetString(0)),

                        FileName =
                            reader.GetString(1),

                        OriginalFileName =
                            reader.GetString(2),

                        FilePath =
                            reader.GetString(3),

                        Category =
                            (DocumentCategoryType)
                            reader.GetInt32(4),

                        Description =
                            reader.IsDBNull(5)
                                ? ""
                                : reader.GetString(5),
                        DocumentNumber =
                                reader["DocumentNumber"]
                                ?.ToString() ?? "",

                        Title =
                                reader["Title"]
                                ?.ToString() ?? "",

                        Status =
                                reader["Status"]
                                ?.ToString() ?? "",

Revision =
    reader["Revision"]
        ?.ToString() ?? "",

                        UploadedDate =
    DateTime.TryParse(
        reader["UploadedDate"]
            ?.ToString(),
        out var uploadedDate)
            ? uploadedDate
            : DateTime.Now,

                        UploadedBy =
    reader["UploadedBy"]
        ?.ToString() ?? "",

                        WeldId =
    string.IsNullOrWhiteSpace(
        reader["WeldId"]
            ?.ToString())
        ? null
        : Guid.Parse(
            reader["WeldId"]
                .ToString()!),

                        ProjectId =
    string.IsNullOrWhiteSpace(
        reader["ProjectId"]
            ?.ToString())
        ? null
        : Guid.Parse(
            reader["ProjectId"]
                .ToString()!),


                        CompanyId =
    string.IsNullOrWhiteSpace(
        reader["CompanyId"]
            ?.ToString())
        ? null
        : Guid.Parse(
            reader["CompanyId"]
                .ToString()!),
                        IsApproved =
    Convert.ToInt32(
        reader["IsApproved"]) == 1

                    });
            }

            return files;
        }
        private bool Exists(
            Guid? projectId,
            string fileName,
            Guid? companyId = null)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            if (projectId.HasValue)
            {
                cmd.CommandText = @"
                SELECT COUNT(*)
                FROM DocumentVaultFiles
                WHERE ProjectId = $ProjectId
                AND FileName = $FileName;";

                cmd.Parameters.AddWithValue(
                    "$ProjectId",
                    projectId.Value.ToString());
            }
            else if (companyId.HasValue)
            {
                cmd.CommandText = @"
                SELECT COUNT(*)
                FROM DocumentVaultFiles
                WHERE ProjectId IS NULL
                AND WeldId IS NULL
                AND CompanyId = $CompanyId
                AND FileName = $FileName;";

                cmd.Parameters.AddWithValue(
                    "$CompanyId",
                    companyId.Value.ToString());
            }
            else
            {
                cmd.CommandText = @"
                SELECT COUNT(*)
                FROM DocumentVaultFiles
                WHERE ProjectId IS NULL
                AND WeldId IS NULL
                AND CompanyId IS NULL
                AND FileName = $FileName;";
            }

            cmd.Parameters.AddWithValue(
                "$FileName",
                fileName);

            return Convert.ToInt32(
                cmd.ExecuteScalar()) > 0;
        }
    }
}




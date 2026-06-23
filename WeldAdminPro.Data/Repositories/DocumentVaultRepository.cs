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
                    file.FileName))
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
                "$IsApproved",
                file.IsApproved ? 1 : 0);

            command.ExecuteNonQuery();
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

                        IsApproved =
    Convert.ToInt32(
        reader["IsApproved"]) == 1

                    });
            }

            return files;
        }
        private bool Exists(
            Guid? projectId,
            string fileName)
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
            else
            {
                cmd.CommandText = @"
                SELECT COUNT(*)
                FROM DocumentVaultFiles
                WHERE ProjectId IS NULL
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

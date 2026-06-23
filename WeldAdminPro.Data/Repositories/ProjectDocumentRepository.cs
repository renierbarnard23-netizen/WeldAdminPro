using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectDocumentRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        // =====================================================
        // MIGRATIONS
        // =====================================================

        private void RunMigrations(SqliteConnection connection)
        {
            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "IsApproved",
                "INTEGER NOT NULL DEFAULT 0");

            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "AllowMultiple",
                "INTEGER NOT NULL DEFAULT 0");

            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "UploadedDate",
                "TEXT");

            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "ApprovedBy",
                "TEXT");

            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "LastModifiedOn",
                "TEXT");

            AddColumnIfMissing(
                connection,
                "ProjectDocuments",
                "Category",
                "TEXT");
        }

        private void AddColumnIfMissing(
            SqliteConnection connection,
            string table,
            string column,
            string definition)
        {
            using var check = connection.CreateCommand();

            check.CommandText =
                $"PRAGMA table_info({table});";

            using var reader = check.ExecuteReader();

            var exists = false;

            while (reader.Read())
            {
                if (reader["name"].ToString() == column)
                {
                    exists = true;
                    break;
                }
            }

            reader.Close();

            if (!exists)
            {
                using var alter = connection.CreateCommand();

                alter.CommandText =
                    $"ALTER TABLE {table} ADD COLUMN {column} {definition};";

                alter.ExecuteNonQuery();
            }
        }

        // =====================================================
        // GET BY PROJECT
        // =====================================================

        public List<ProjectDocument> GetByProject(Guid projectId)
        {
            var list = new List<ProjectDocument>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            RunMigrations(connection);

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    ProjectId,
    DocumentType,
    IsRequired,
    IsUploaded,
    FilePath,
    UploadedDate,
    IsApproved,
    AllowMultiple,
    ApprovedBy,
    LastModifiedOn,
    Category
FROM ProjectDocuments
WHERE ProjectId = $projectId;";

            cmd.Parameters.AddWithValue(
                "$projectId",
                projectId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var docType =
                    reader.IsDBNull(2)
                        ? "Unknown"
                        : reader.GetString(2);

                DateTime? uploadedDate = null;

                if (!reader.IsDBNull(6))
                {
                    var raw = reader.GetString(6);

                    if (DateTime.TryParse(raw, out var parsed))
                        uploadedDate = parsed;
                }

                list.Add(new ProjectDocument
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    ProjectId = Guid.Parse(reader.GetString(1)),

                    DocumentType = docType,
                    DocumentName = docType,

                    IsRequired =
                        !reader.IsDBNull(3) &&
                        reader.GetInt32(3) == 1,

                    IsUploaded =
                        !reader.IsDBNull(4) &&
                        reader.GetInt32(4) == 1,

                    FilePath =
                        reader.IsDBNull(5)
                            ? ""
                            : reader.GetString(5),

                    UploadedDate = uploadedDate,

                    IsApproved =
                        !reader.IsDBNull(7) &&
                        reader.GetInt32(7) == 1,

                    AllowMultiple =
                        !reader.IsDBNull(8) &&
                        reader.GetInt32(8) == 1,

                    ApprovedBy =
                        reader.IsDBNull(9)
                        ? ""
                        : reader.GetString(9),

                    LastModifiedOn =
                        reader.IsDBNull(10)
                        ? null
                        : DateTime.Parse(
                        reader.GetString(10)),

                    Category =
                        reader.IsDBNull(11)
                        ? ""
                        : reader.GetString(11)


                });
            }

            return list;
        }

        // =====================================================
        // ADD
        // =====================================================

        public void Add(ProjectDocument doc)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            RunMigrations(connection);

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO ProjectDocuments
(
    Id,
    ProjectId,
    DocumentType,
    IsRequired,
    IsUploaded,
    FilePath,
    UploadedDate,
    IsApproved,
    AllowMultiple,
    ApprovedBy,
    LastModifiedOn,
    Category
)

VALUES
(
    $id,
    $projectId,
    $type,
    $required,
    $uploaded,
    $path,
    $date,
    $approved,
    $multiple,
    $approvedBy,
    $lastModified,
    $category
);";

            cmd.Parameters.AddWithValue(
                "$id",
                doc.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$projectId",
                doc.ProjectId.ToString());

            cmd.Parameters.AddWithValue(
                "$type",
                doc.DocumentType);

            cmd.Parameters.AddWithValue(
                "$required",
                doc.IsRequired ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$uploaded",
                doc.IsUploaded ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$path",
                string.IsNullOrEmpty(doc.FilePath)
                    ? DBNull.Value
                    : doc.FilePath);

            cmd.Parameters.AddWithValue(
                "$date",
                doc.UploadedDate.HasValue
                    ? doc.UploadedDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$approved",
                doc.IsApproved ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$multiple",
                doc.AllowMultiple ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$approvedBy",
                doc.ApprovedBy);

            cmd.Parameters.AddWithValue(
                "$lastModified",
                doc.LastModifiedOn.HasValue
                    ? doc.LastModifiedOn.Value.ToString("O")
                    : DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public void Update(ProjectDocument doc)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            RunMigrations(connection);

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE ProjectDocuments
SET
    IsRequired = $required,
    IsUploaded = $uploaded,
    FilePath = $path,
    UploadedDate = $date,
    IsApproved = $approved,
    AllowMultiple = $multiple,
    ApprovedBy = $approvedBy,
    LastModifiedOn = $lastModified,
    Category = $category
WHERE Id = $id;";

            cmd.Parameters.AddWithValue(
                "$id",
                doc.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$required",
                doc.IsRequired ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$uploaded",
                doc.IsUploaded ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$path",
                string.IsNullOrEmpty(doc.FilePath)
                    ? DBNull.Value
                    : doc.FilePath);

            cmd.Parameters.AddWithValue(
                "$date",
                doc.UploadedDate.HasValue
                    ? doc.UploadedDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$approved",
                doc.IsApproved ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$multiple",
                doc.AllowMultiple ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$approvedBy",
                doc.ApprovedBy);

            cmd.Parameters.AddWithValue(
                "$lastModified",
                doc.LastModifiedOn.HasValue
                    ? doc.LastModifiedOn.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$category",
                doc.Category);

            cmd.ExecuteNonQuery();
        }
    }
}
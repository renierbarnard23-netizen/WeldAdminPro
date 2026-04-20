using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectDocumentRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        public List<ProjectDocument> GetByProject(Guid projectId)
        {
            var list = new List<ProjectDocument>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM ProjectDocuments WHERE ProjectId = $projectId;";
            cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DateTime? uploadedDate = null;

                if (!reader.IsDBNull(6))
                {
                    var raw = reader.GetString(6);

                    // ✅ SAFE PARSE
                    if (DateTime.TryParse(raw, out var parsed))
                        uploadedDate = parsed;
                    else
                        uploadedDate = null; // ignore bad data
                }

                list.Add(new ProjectDocument
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    ProjectId = Guid.Parse(reader.GetString(1)),

                    DocumentType = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),

                    IsRequired = !reader.IsDBNull(3) && reader.GetInt32(3) == 1,
                    IsUploaded = !reader.IsDBNull(4) && reader.GetInt32(4) == 1,

                    FilePath = reader.IsDBNull(5) ? "" : reader.GetString(5),

                    // ✅ FIXED
                    UploadedDate = uploadedDate
                });
            }

            return list;
        }

        public void Add(ProjectDocument doc)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ProjectDocuments 
(Id, ProjectId, DocumentType, IsRequired, IsUploaded, FilePath, UploadedDate)
VALUES ($id, $projectId, $type, $required, $uploaded, $path, $date);";

            cmd.Parameters.AddWithValue("$id", doc.Id.ToString());
            cmd.Parameters.AddWithValue("$projectId", doc.ProjectId.ToString());
            cmd.Parameters.AddWithValue("$type", doc.DocumentType);
            cmd.Parameters.AddWithValue("$required", doc.IsRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("$uploaded", doc.IsUploaded ? 1 : 0);
            cmd.Parameters.AddWithValue("$path",
                string.IsNullOrEmpty(doc.FilePath) ? DBNull.Value : doc.FilePath);
            cmd.Parameters.AddWithValue("$date",
                doc.UploadedDate.HasValue
                    ? doc.UploadedDate.Value.ToString("yyyy-MM-dd")
                    : DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Update(ProjectDocument doc)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE ProjectDocuments
SET IsUploaded = $uploaded,
    FilePath = $path,
    UploadedDate = $date
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", doc.Id.ToString());
            cmd.Parameters.AddWithValue("$uploaded", doc.IsUploaded ? 1 : 0);
            cmd.Parameters.AddWithValue("$path",
                string.IsNullOrEmpty(doc.FilePath) ? DBNull.Value : doc.FilePath);
            cmd.Parameters.AddWithValue("$date",
                doc.UploadedDate.HasValue
                    ? doc.UploadedDate.Value.ToString("yyyy-MM-dd")
                    : DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}
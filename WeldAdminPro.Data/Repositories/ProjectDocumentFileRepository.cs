using System.Data;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectDocumentFileRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        // =====================================================
        // ADD FILE
        // =====================================================

        public void Add(ProjectDocumentFile file)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO ProjectDocumentFiles
(
    Id,
    ProjectDocumentId,
    FilePath,
    FileName,
    UploadedOn,
    IsApproved
)
VALUES
(
    $id,
    $docId,
    $path,
    $name,
    $uploaded,
    $approved
);";

            cmd.Parameters.AddWithValue(
                "$id",
                file.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$docId",
                file.ProjectDocumentId.ToString());

            cmd.Parameters.AddWithValue(
                "$path",
                file.FilePath);

            cmd.Parameters.AddWithValue(
                "$name",
                file.FileName);

            cmd.Parameters.AddWithValue(
                "$uploaded",
                file.UploadedOn.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.Parameters.AddWithValue(
                "$approved",
                file.IsApproved ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // UPDATE FILE
        // =====================================================

        public void Update(ProjectDocumentFile file)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE ProjectDocumentFiles
SET
    FilePath = $path,
    FileName = $name,
    UploadedOn = $uploaded,
    IsApproved = $approved
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", file.Id.ToString());
            cmd.Parameters.AddWithValue("$path", file.FilePath);
            cmd.Parameters.AddWithValue("$name", file.FileName);
            cmd.Parameters.AddWithValue(
                "$uploaded",
                file.UploadedOn.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.Parameters.AddWithValue(
                "$approved",
                file.IsApproved ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // GET FILES BY DOCUMENT
        // =====================================================

        public List<ProjectDocumentFile> GetByDocument(Guid documentId)
        {
            var list = new List<ProjectDocumentFile>();

            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    ProjectDocumentId,
    FilePath,
    FileName,
    UploadedOn,
    IsApproved
FROM ProjectDocumentFiles
WHERE ProjectDocumentId = $id;";

            cmd.Parameters.AddWithValue(
                "$id",
                documentId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new ProjectDocumentFile
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    ProjectDocumentId = Guid.Parse(reader.GetString(1)),
                    FilePath = reader.GetString(2),
                    FileName = reader.GetString(3),
                    UploadedOn = DateTime.Parse(reader.GetString(4)),
                    IsApproved = reader.GetInt32(5) == 1
                });
            }

            return list;
        }
    }
}
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectRepository
    {
        private readonly string _connectionString =
            "Data Source=weldadmin.db";

        public ProjectRepository()
        {
            EnsureDatabase();
        }

        private void EnsureDatabase()
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    // Create table if it does not exist (legacy DBs may miss columns)
    var createCmd = connection.CreateCommand();
    createCmd.CommandText =
    """
    CREATE TABLE IF NOT EXISTS Projects (
        Id TEXT PRIMARY KEY,
        ProjectName TEXT NOT NULL,
        StartDate TEXT,
        EndDate TEXT
    );
    """;
    createCmd.ExecuteNonQuery();

    // Add ProjectNumber if missing
    var addProjectNumberCmd = connection.CreateCommand();
    addProjectNumberCmd.CommandText =
    """
    ALTER TABLE Projects ADD COLUMN ProjectNumber TEXT;
    """;

    try
    {
        addProjectNumberCmd.ExecuteNonQuery();
    }
    catch
    {
        // Column already exists → safe to ignore
    }
}




        public List<Project> GetAll()
        {
            var list = new List<Project>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
"""
SELECT
    Id,
    ProjectNumber,
    ProjectName,
    StartDate,
    EndDate
FROM Projects;
""";


            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Project
{
    Id = Guid.Parse(reader.GetString(0)),
    ProjectNumber = reader.GetString(1),
    ProjectName = reader.GetString(2),
    StartDate = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
    EndDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4))
});
            }

            return list;
        }
public void Update(Project project)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    """
    UPDATE Projects
    SET
        ProjectName = $name,
        StartDate = $start,
        EndDate = $end
    WHERE Id = $id;
    """;

    cmd.Parameters.AddWithValue("$id", project.Id.ToString());
    cmd.Parameters.AddWithValue("$name", project.ProjectName);

    cmd.Parameters.AddWithValue(
        "$start",
        project.StartDate.HasValue
            ? project.StartDate.Value.ToString("o")
            : DBNull.Value
    );

    cmd.Parameters.AddWithValue(
        "$end",
        project.EndDate.HasValue
            ? project.EndDate.Value.ToString("o")
            : DBNull.Value
    );

    cmd.ExecuteNonQuery();
}


public void Add(Project project)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    """
    INSERT INTO Projects (
        Id,
        ProjectNumber,
        ProjectName,
        StartDate,
        EndDate
    )
    VALUES (
        $id,
        $number,
        $name,
        $start,
        $end
    );
    """;

    // 🔒 GUARANTEED VALUES
    cmd.Parameters.AddWithValue("$id", project.Id.ToString());
    cmd.Parameters.AddWithValue("$number", project.ProjectNumber ?? "");
    cmd.Parameters.AddWithValue("$name", project.ProjectName ?? "");
    cmd.Parameters.AddWithValue(
        "$start",
        project.StartDate.HasValue ? project.StartDate.Value.ToString("o") : DBNull.Value
    );
    cmd.Parameters.AddWithValue(
        "$end",
        project.EndDate.HasValue ? project.EndDate.Value.ToString("o") : DBNull.Value
    );

    cmd.ExecuteNonQuery();
}

    }

}

using Microsoft.Data.Sqlite;

namespace WeldAdminPro.Data
{
    public static class WeldAdminDbInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS projects (
                    id TEXT PRIMARY KEY,
                    code TEXT NOT NULL UNIQUE,
                    name TEXT NOT NULL,
                    created_at TEXT NOT NULL
                );
            ";

            cmd.ExecuteNonQuery();
        }
    }
}

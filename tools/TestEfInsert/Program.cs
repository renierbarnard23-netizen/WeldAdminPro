@'
using System;
using System.IO;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main()
    {
        try
        {
            // find the repo-root weldadmin.db (walk upwards from AppContext.BaseDirectory & CWD)
            string dbName = "weldadmin.db";
            string? dbPath = FindDatabase(dbName);
            if (dbPath == null)
            {
                Console.Error.WriteLine($"Could not find {dbName} in repo. Run Get-ChildItem -Recurse to locate it.");
                return 2;
            }

            Console.WriteLine($"Using DB: {dbPath}");
            var connString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

            using var conn = new SqliteConnection(connString);
            conn.Open();

            Console.WriteLine("=== PRAGMA table_info('users') ===");
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info('users');";
                using var rd = cmd.ExecuteReader();
                Console.WriteLine("cid | name | type | notnull | dflt_value | pk");
                while (rd.Read())
                {
                    Console.WriteLine($"{rd["cid"]} | {rd["name"]} | {rd["type"]} | {rd["notnull"]} | {rd["dflt_value"]} | {rd["pk"]}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Inserting test user via SQL (created_at & updated_at set via datetime('now'))...");

            using (var tx = conn.BeginTransaction())
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO users (username, email, created_at, updated_at)
VALUES (
  'sql_test_user_' || strftime('%Y%m%d%H%M%S','now'),
  'sqltest@local',
  datetime('now'),
  datetime('now')
);
";
                var rows = cmd.ExecuteNonQuery();
                Console.WriteLine($"Inserted rows: {rows}");
                tx.Commit();
            }

            Console.WriteLine();
            Console.WriteLine("=== Last 20 rows from users (id, username, created_at, updated_at) ===");
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT id, username, created_at, updated_at
FROM users
ORDER BY id DESC
LIMIT 20;
";
                using var rd = cmd.ExecuteReader();
                Console.WriteLine("id | username | created_at | updated_at");
                while (rd.Read())
                {
                    var id = rd["id"];
                    var username = rd["username"] is DBNull ? "(null)" : rd["username"].ToString();
                    var ca = rd["created_at"] is DBNull ? "(NULL)" : (rd["created_at"]?.ToString() ?? "(empty)");
                    var ua = rd["updated_at"] is DBNull ? "(NULL)" : (rd["updated_at"]?.ToString() ?? "(empty)");
                    Console.WriteLine($"{id} | {username} | {ca} | {ua}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex);
            return 1;
        }
    }

    static string? FindDatabase(string fileName)
    {
        var startPaths = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var start in startPaths)
        {
            var p = new DirectoryInfo(start);
            while (p != null)
            {
                var candidate = Path.Combine(p.FullName, fileName);
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
                p = p.Parent;
            }
        }
        // fallback: repo root sibling guess (safe)
        var fallback = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", fileName));
        if (File.Exists(fallback)) return fallback;
        return null;
    }
}
'@ | Set-Content -Path .\Tools\TempRunner\Program.cs -Encoding utf8

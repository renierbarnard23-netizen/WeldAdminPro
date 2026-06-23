using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        var dbPath = "weldadmin.db"; // adjust if needed
        if (!System.IO.File.Exists(dbPath))
        {
            Console.WriteLine($"File not found: {dbPath}");
            return;
        }

        var connString = $"Data Source={dbPath}";
        using var conn = new SqliteConnection(connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name, type FROM sqlite_master WHERE type IN ('table','view') ORDER BY name;";
        using var rdr = cmd.ExecuteReader();
        Console.WriteLine($"Objects in {dbPath}:");
        while (rdr.Read())
        {
            Console.WriteLine($"{rdr.GetString(0)}   ({rdr.GetString(1)})");
        }

        // Example: show row count for Projects table if it exists
        cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Projects';";
        var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        if (exists)
        {
            using var c2 = conn.CreateCommand();
            c2.CommandText = "SELECT count(*) FROM Projects;";
            var ct = Convert.ToInt32(c2.ExecuteScalar());
            Console.WriteLine($"\nProjects row count: {ct}");
        }
    }
}

using System;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "weldadmin.db";
        Console.WriteLine("Opening database: " + dbPath);
        try
        {
            using var conn = new SqliteConnection("Data Source=" + dbPath);
            conn.Open();

            Console.WriteLine();
            Console.WriteLine("== Tables ==");
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read()) Console.WriteLine(" - " + rdr.GetString(0));
            }

            Console.WriteLine();
            Console.WriteLine("== jobs table schema (PRAGMA table_info('jobs')) ==");
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info('jobs');";
                    using var rdr = cmd.ExecuteReader();
                    Console.WriteLine("cid | name | type | notnull | dflt_value | pk");
                    while (rdr.Read())
                    {
                        string dflt = rdr.IsDBNull(4) ? "NULL" : rdr.GetValue(4).ToString();
                        Console.WriteLine(rdr.GetInt32(0) + " | " + rdr.GetString(1) + " | " + rdr.GetString(2) + " | " + rdr.GetInt32(3) + " | " + dflt + " | " + rdr.GetInt32(5));
                    }
                }
            }
            catch (Exception exSchema)
            {
                Console.WriteLine("Could not read jobs schema: " + exSchema.Message);
            }

            Console.WriteLine();
            Console.WriteLine("== Row counts ==");
            string[] tables = new[] { "users","projects","jobs","job_history","attachments" };
            foreach (var t in tables)
            {
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM " + t + ";";
                    var cnt = cmd.ExecuteScalar();
                    Console.WriteLine(" " + t + ": " + cnt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" " + t + ": (error) " + ex.Message);
                }
            }

            conn.Close();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            return 2;
        }
    }
}

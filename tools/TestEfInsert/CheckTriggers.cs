// Tools/TestEfInsert/CheckTriggers.cs
using System;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main()
    {
        try
        {
            using var conn = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = "weldadmin.db" }.ToString());
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='trigger' AND tbl_name='users';";
            using var rd = cmd.ExecuteReader();
            Console.WriteLine("Triggers on table 'users':");
            var found = false;
            while (rd.Read())
            {
                found = true;
                Console.WriteLine($"- {rd["name"]}: {rd["sql"]}");
            }
            if (!found) Console.WriteLine("(none)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex);
            return 1;
        }
    }
}

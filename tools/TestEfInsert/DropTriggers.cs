// Tools/TestEfInsert/DropTriggers.cs
using System;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main()
    {
        try
        {
            var dbPath = "weldadmin.db";
            var connString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

            using var conn = new SqliteConnection(connString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
DROP TRIGGER IF EXISTS trg_users_after_insert;
DROP TRIGGER IF EXISTS trg_users_after_update;
";
            cmd.ExecuteNonQuery();
            Console.WriteLine("Triggers dropped (if they existed).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to drop triggers: " + ex);
            return 1;
        }
    }
}

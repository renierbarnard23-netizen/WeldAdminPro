// Tools/TestEfInsert/ApplyTriggers.cs
using System;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            var dbPath = "weldadmin.db";
            var connString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

            using var conn = new SqliteConnection(connString);
            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
CREATE TRIGGER IF NOT EXISTS trg_users_after_insert
AFTER INSERT ON users
WHEN NEW.updated_at IS NULL OR trim(NEW.updated_at) = ''
BEGIN
  UPDATE users
  SET updated_at = COALESCE(NEW.created_at, datetime('now'))
  WHERE id = NEW.id;
END;

CREATE TRIGGER IF NOT EXISTS trg_users_after_update
AFTER UPDATE ON users
WHEN NEW.updated_at IS NULL OR trim(NEW.updated_at) = ''
BEGIN
  UPDATE users
  SET updated_at = datetime('now')
  WHERE id = NEW.id;
END;
";
            cmd.ExecuteNonQuery();

            Console.WriteLine("Triggers created (or already existed).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to apply triggers: " + ex);
            return 1;
        }
    }
}

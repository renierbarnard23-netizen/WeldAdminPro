using System;
using System.IO;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run --project tools/applymig -- <sql-file> <db-file>");
            return 2;
        }

        var sqlFile = args[0];
        var dbFile = args[1];

        if (!File.Exists(sqlFile))
        {
            Console.WriteLine("SQL file not found: " + sqlFile);
            return 3;
        }
        if (!File.Exists(dbFile))
        {
            Console.WriteLine("DB file not found: " + dbFile);
            return 4;
        }

        var sql = File.ReadAllText(sqlFile);
        try
        {
            using var conn = new SqliteConnection("Data Source=" + dbFile);
            conn.Open();
            using var txn = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = txn;

            // Split by semicolon and execute statements individually to avoid problems with some PRAGMA/BEGIN blocks
            var parts = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var s = part.Trim();
                if (string.IsNullOrWhiteSpace(s)) continue;
                cmd.CommandText = s + ";";
                cmd.ExecuteNonQuery();
            }

            txn.Commit();
            conn.Close();
            Console.WriteLine("SQL applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR applying SQL: " + ex.Message);
            return 5;
        }
    }
}

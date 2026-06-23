using System;
using Microsoft.Data.Sqlite;

class Program
{
    static int Main(string[] args)
    {
        var db = args.Length > 0 ? args[0] : "weldadmin.db";
        var cs = "Data Source=" + db;
        try
        {
            using var conn = new SqliteConnection(cs);
            conn.Open();

            void Query(string q)
            {
                Console.WriteLine("---- " + q);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = q;
                using var r = cmd.ExecuteReader();

                // print header
                for (int i = 0; i < r.FieldCount; i++)
                {
                    Console.Write(r.GetName(i) + (i == r.FieldCount - 1 ? "\n" : " | "));
                }

                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                    {
                        var v = r.IsDBNull(i) ? "NULL" : r.GetValue(i).ToString();
                        Console.Write(v + (i == r.FieldCount - 1 ? "\n" : " | "));
                    }
                }
                Console.WriteLine();
            }

            Query("SELECT * FROM users;");
            Query("SELECT * FROM projects;");
            Query("SELECT * FROM jobs;");
            Query("SELECT * FROM job_history;");
            Query("SELECT * FROM attachments;");

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

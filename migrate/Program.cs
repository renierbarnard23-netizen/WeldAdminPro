using System;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Data; // ensure this matches the Data project's namespace

namespace MigrateRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Creating DbContext options (SQLite: ../weldadmin.db)...");
                var options = new DbContextOptionsBuilder<WeldAdminDbContext>()
                    .UseSqlite("Data Source=../weldadmin.db")
                    .Options;

                using var ctx = new WeldAdminDbContext(options);

                Console.WriteLine("Applying EF Core migrations...");
                ctx.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Migration runner failed:");
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}

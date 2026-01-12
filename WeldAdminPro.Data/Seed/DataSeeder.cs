using System;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data
{
    public static class DataSeeder
    {
        public static void Seed(WeldAdminDbContext db)
        {
            if (db.Projects.Any()) return;

            var proj = new Project
            {
                Id = Guid.NewGuid(), // ✅ EXPLICIT
                ProjectNumber = "PRJ-001",
                Title = "Initial Test Project",
                Client = "Dynamic Options Engineering",
                StartDate = DateTime.UtcNow.Date,
                DatabookVersion = "v1",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            db.Projects.Add(proj);
            db.SaveChanges();
        }
    }
}

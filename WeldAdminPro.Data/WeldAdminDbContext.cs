using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data
{
    public class WeldAdminDbContext : DbContext
    {
        public WeldAdminDbContext(DbContextOptions<WeldAdminDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Wps> Wpss { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasIndex(p => p.ProjectNumber).IsUnique();
            modelBuilder.Entity<Document>().HasIndex(d => new { d.ProjectId, d.DocumentNumber });
            base.OnModelCreating(modelBuilder);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data
{
    public class WeldAdminDbContext : DbContext
    {
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Wps> Wpss => Set<Wps>();

        public WeldAdminDbContext() { }

        public WeldAdminDbContext(DbContextOptions<WeldAdminDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite(
                    @"Data Source=C:\Users\renie\Documents\WeldAdminPro\weldadmin.db"
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).HasColumnType("TEXT");
                e.Property(p => p.ProjectNumber).IsRequired();
                e.Property(p => p.Title).IsRequired();
            });

            modelBuilder.Entity<Document>(e =>
            {
                e.HasKey(d => d.Id);
                e.Property(d => d.Id).HasColumnType("TEXT");
            });

            modelBuilder.Entity<Wps>(e =>
            {
                e.HasKey(w => w.Id);
                e.Property(w => w.Id).HasColumnType("TEXT");
            });
        }
    }
}

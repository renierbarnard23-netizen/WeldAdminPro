using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using Microsoft.Data.Sqlite;
using System.Diagnostics;



namespace WeldAdminPro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	: base(options)
		{
			LogDatabasePath();
		}

		public DbSet<ProjectStockUsage> ProjectStockUsages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // common DateTime <-> TEXT converter using SQLite-friendly format
            var dateTimeConverter = new ValueConverter<DateTime?, string?>(
                v => v.HasValue ? v.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") : null,
                v => v != null ? DateTime.SpecifyKind(DateTime.ParseExact(v, "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc) : (DateTime?)null
            );

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users"); // explicit mapping to existing table name

                b.HasKey(u => u.Id);
                b.Property(u => u.Username).HasColumnName("username").IsRequired();
                b.Property(u => u.DisplayName).HasColumnName("display_name");
                b.Property(u => u.Email).HasColumnName("email");
                b.Property(u => u.PasswordHash).HasColumnName("password_hash");
                b.Property(u => u.Role).HasColumnName("role");

                // created_at column mapping: TEXT, default now
                b.Property(u => u.CreatedAt)
                 .HasColumnName("created_at")
                 .HasConversion(dateTimeConverter)
                 .HasColumnType("TEXT")
                 .HasDefaultValueSql("datetime('now')");

                // updated_at column mapping
                b.Property(u => u.UpdatedAt)
                 .HasColumnName("updated_at")
                 .HasConversion(dateTimeConverter)
                 .HasColumnType("TEXT");
            });

            base.OnModelCreating(modelBuilder); }

			private void LogDatabasePath()
		{
			try
			{
				var connection = Database.GetDbConnection();

				if (connection is Microsoft.Data.Sqlite.SqliteConnection sqlite)
				{
					Debug.WriteLine("======================================");
					Debug.WriteLine("USING SQLITE DATABASE FILE:");
					Debug.WriteLine(sqlite.DataSource);
					Debug.WriteLine("======================================");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to determine database path:");
				Debug.WriteLine(ex.Message);
			}
		}

		

        

        // Automatically set timestamps in C# to keep app behavior consistent
        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            // For newly added entities
            var added = ChangeTracker.Entries()
                        .Where(e => e.State == EntityState.Added);

            foreach (var e in added)
            {
                var propCreated = e.Metadata.FindProperty(nameof(User.CreatedAt));
                if (propCreated != null && (e.Property("CreatedAt").CurrentValue == null))
                {
                    e.Property("CreatedAt").CurrentValue = utcNow;
                }
            }

            // For modified entities
            var modified = ChangeTracker.Entries()
                           .Where(e => e.State == EntityState.Modified);

            foreach (var e in modified)
            {
                var propUpdated = e.Metadata.FindProperty(nameof(User.UpdatedAt));
                if (propUpdated != null)
                {
                    e.Property("UpdatedAt").CurrentValue = utcNow;
                }
            }
        }
    }
}

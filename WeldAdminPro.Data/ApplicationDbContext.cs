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
		public DbSet<ProjectStockUsage> ProjectStockUsages { get; set; } = null!;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
			EnsureProjectStockUsageTable();
			LogDatabasePath();
		}

		private void EnsureProjectStockUsageTable()
		{
			// Create table if it does not exist
			Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ProjectStockUsages (
            Id TEXT PRIMARY KEY,
            ProjectId TEXT NOT NULL,
            StockItemId TEXT NOT NULL,
            Quantity REAL NOT NULL,
            IssuedOn TEXT NOT NULL,
            IssuedBy TEXT,
            Notes TEXT
        );
    ");

			// Ensure Notes column exists (SQLite-safe)
			try
			{
				Database.ExecuteSqlRaw(
					"ALTER TABLE ProjectStockUsages ADD COLUMN Notes TEXT;"
				);
			}
			catch
			{
				// Column already exists → ignore
			}
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var dateTimeConverter = new ValueConverter<DateTime?, string?>(
				v => v.HasValue ? v.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") : null,
				v => v != null ? DateTime.SpecifyKind(DateTime.ParseExact(v, "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc) : (DateTime?)null
			);

			modelBuilder.Entity<User>(b =>
			{
				b.ToTable("users");
				b.HasKey(u => u.Id);
				b.Property(u => u.Username).HasColumnName("username").IsRequired();
				b.Property(u => u.DisplayName).HasColumnName("display_name");
				b.Property(u => u.Email).HasColumnName("email");
				b.Property(u => u.PasswordHash).HasColumnName("password_hash");
				b.Property(u => u.Role).HasColumnName("role");
				b.Property(u => u.CreatedAt).HasColumnName("created_at").HasConversion(dateTimeConverter).HasColumnType("TEXT");
				b.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasConversion(dateTimeConverter).HasColumnType("TEXT");
			});

			base.OnModelCreating(modelBuilder);
		}

		private void LogDatabasePath()
		{
			try
			{
				var connection = Database.GetDbConnection();
				if (connection is SqliteConnection sqlite)
				{
					Debug.WriteLine("Using DB: " + sqlite.DataSource);
				}
			}
			catch { }
		}

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

			foreach (var e in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
			{
				if (e.Metadata.FindProperty(nameof(User.CreatedAt)) != null)
					e.Property("CreatedAt").CurrentValue ??= utcNow;
			}

			foreach (var e in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
			{
				if (e.Metadata.FindProperty(nameof(User.UpdatedAt)) != null)
					e.Property("UpdatedAt").CurrentValue = utcNow;
			}
		}
	}
}

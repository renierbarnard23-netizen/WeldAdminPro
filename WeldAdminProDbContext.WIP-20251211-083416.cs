// File: WeldAdminPro.Data\WeldAdminProDbContext.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data
{
    // Minimal, safe DbContext. This compiles and maps the users table to Core's User model.
    public class WeldAdminProDbContext : DbContext
    {
        public WeldAdminProDbContext(DbContextOptions<WeldAdminProDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).HasColumnName("id");
                b.Property(u => u.Username).HasColumnName("username").IsRequired();
                b.Property(u => u.DisplayName).HasColumnName("display_name");
                b.Property(u => u.Email).HasColumnName("email");
                b.Property(u => u.PasswordHash).HasColumnName("password_hash");
                b.Property(u => u.Role).HasColumnName("role");
                b.Property(u => u.CreatedAt).HasColumnName("created_at");
                b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            });

            base.OnModelCreating(modelBuilder);
        }

        // --- Fix 3: Automatic timestamps for CreatedAt / UpdatedAt ---

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Applies CreatedAt (for Added entries) and UpdatedAt (for Added or Modified entries).
        /// Resilient to User.CreatedAt/UpdatedAt being DateTime/DateTime? or string.
        /// </summary>
        private void ApplyTimestamps()
        {
            var now = DateTime.UtcNow;

            // Pre-fetch PropertyInfo for a tiny performance boost
            var userType = typeof(User);
            var createdProp = userType.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            var updatedProp = userType.GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance);

            foreach (var entry in ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var entity = entry.Entity;

                // SET CreatedAt for newly added entities
                if (entry.State == EntityState.Added && createdProp != null)
                {
                    SetPropertyValue(createdProp, entity, now);
                }

                // SET UpdatedAt for added or modified entities
                if (updatedProp != null)
                {
                    SetPropertyValue(updatedProp, entity, now);
                }
            }
        }

        /// <summary>
        /// Sets a property to a DateTime value or to a formatted string depending on the property's type.
        /// </summary>
        private void SetPropertyValue(PropertyInfo prop, object entity, DateTime utcNow)
        {
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (targetType == typeof(DateTime))
            {
                prop.SetValue(entity, utcNow);
                return;
            }

            if (targetType == typeof(string))
            {
                // Use a compact, readable format similar to SQLite CURRENT_TIMESTAMP
                prop.SetValue(entity, utcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                return;
            }

            // Fallback: try to convert if possible
            try
            {
                var converted = Convert.ChangeType(utcNow, targetType);
                prop.SetValue(entity, converted);
            }
            catch
            {
                // If we can't convert, ignore silently — avoid throwing during SaveChanges.
            }
        }
    }
}

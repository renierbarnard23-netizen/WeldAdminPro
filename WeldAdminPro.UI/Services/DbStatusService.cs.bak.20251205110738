using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace WeldAdminPro.UI.Services
{
    /// <summary>
    /// Small helper that reads the __EFMigrationsHistory table (if present) and reports a simple status string.
    /// This isolates DB-specific code from viewmodels and can be used by MainViewModel at runtime via reflection.
    /// </summary>
    public class DbStatusService
    {
        private readonly string _dbPath;

        public DbStatusService(string dbPath = "weldadmin.db")
        {
            _dbPath = dbPath;
        }

        public async Task<string> GetMigrationStatusAsync()
        {
            try
            {
                if (!File.Exists(_dbPath)) return "DB missing";

                var cs = $"Data Source={_dbPath}";
                using var conn = new SqliteConnection(cs);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
                var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                if (!exists) return "No migrations table";

                cmd.CommandText = "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1;";
                using var rdr = await cmd.ExecuteReaderAsync();
                if (!await rdr.ReadAsync()) return "No migrations applied";

                var id = rdr.GetString(0);
                var ver = rdr.FieldCount > 1 ? rdr.GetString(1) : "";
                return $"Last migration: {id} ({ver})";
            }
            catch (Exception ex)
            {
                return "DB status error: " + ex.Message;
            }
        }
    }
}

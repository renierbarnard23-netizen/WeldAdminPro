// File: WeldAdminPro.UI\App.xaml.cs
using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using WeldAdminPro.UI.Views;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // Example: read connection string from appsettings.json or use fallback
                        var connString = context.Configuration["ConnectionStrings:DefaultConnection"]
                                         ?? "Data Source=weldadmin.db";

                        // Register EF DbContext (SQLite example)
                        services.AddDbContext<WeldAdminPro.Data.WeldAdminProDbContext>(options =>
                            options.UseSqlite(connString));

                        // Repositories
                        services.AddScoped<IUserRepository, UserRepository>();

                        // ViewModels
                        services.AddTransient<LoginViewModel>();

                        // Views
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                    })
                    .Build();

                // Optional: ensure DB created and fix schema if needed
                using (var scope = _host.Services.CreateScope())
                {
                    var provider = scope.ServiceProvider;
                    try
                    {
                        var db = provider.GetService<WeldAdminPro.Data.WeldAdminProDbContext>();
                        if (db != null)
                        {
                            db.Database.EnsureCreated();
                        }

                        // Path to sqlite file used above (matches fallback)
                        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "weldadmin.db");

                        // Optional: dump the Users table info to a text file for debugging
                        DumpUsersTableInfo(dbPath);

                        // Ensure CreatedAt column exists and is populated
                        EnsureCreatedAtColumn(dbPath);
                    }
                    catch (Exception ex)
                    {
                        // Log DB-init related errors
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB_init_error.txt"), ex.ToString());
                        // continue — the app will surface errors later if DB is required
                    }
                }

                // Show login window via DI
                ShowLoginWindowSafely();
            }
            catch (Exception ex)
            {
                try
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_OnStartup_error.txt");
                    File.WriteAllText(path, ex.ToString());
                }
                catch { /* ignore write errors */ }

                throw;
            }
        }

        private void ShowLoginWindowSafely()
        {
            try
            {
                if (_host == null)
                    throw new InvalidOperationException("Host not built.");

                var loginVm = _host.Services.GetRequiredService<LoginViewModel>();
                var loginWindow = _host.Services.GetRequiredService<LoginWindow>();

                loginWindow.DataContext = loginVm;

                loginVm.LoginSucceeded += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            loginWindow.Close();
                            var main = _host.Services.GetRequiredService<MainWindow>();
                            this.MainWindow = main;
                            main.Show();
                        }
                        catch (Exception ex)
                        {
                            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenMainWindow_error.txt");
                            File.WriteAllText(path, ex.ToString());
                            MessageBox.Show("Failed to open main window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Shutdown();
                        }
                    });
                };

                loginWindow.Closed += (s, args) =>
                {
                    if (loginWindow.DataContext is LoginViewModel vm && !vm.IsAuthenticated)
                    {
                        Shutdown();
                    }
                };

                loginWindow.Show();
            }
            catch (Exception ex)
            {
                try
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ShowLoginWindow_error.txt");
                    File.WriteAllText(path, ex.ToString());
                }
                catch { }

                throw;
            }
        }

        /// <summary>
        /// Ensures the Users table has a CreatedAt TEXT column.
        /// Adds the column (no non-constant default allowed), then populates existing rows.
        /// </summary>
        /// <param name="dbPath">Path to the sqlite DB file</param>
        private void EnsureCreatedAtColumn(string dbPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath))
                    return;

                var connString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
                using var conn = new SqliteConnection(connString);
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info('Users');";
                    using var reader = cmd.ExecuteReader();
                    bool hasCreatedAt = false;
                    while (reader.Read())
                    {
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        if (string.Equals(name, "CreatedAt", StringComparison.OrdinalIgnoreCase))
                        {
                            hasCreatedAt = true;
                            break;
                        }
                    }

                    if (!hasCreatedAt)
                    {
                        using var alter = conn.CreateCommand();
                        alter.CommandText = "ALTER TABLE Users ADD COLUMN CreatedAt TEXT;";
                        alter.ExecuteNonQuery();

                        using var update = conn.CreateCommand();
                        update.CommandText = "UPDATE Users SET CreatedAt = datetime('now') WHERE CreatedAt IS NULL;";
                        update.ExecuteNonQuery();
                    }
                }

                using (var vac = conn.CreateCommand())
                {
                    vac.CommandText = "VACUUM;";
                    vac.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Save any error so you can inspect it if needed
                try
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnsureCreatedAt_error.txt"), ex.ToString());
                }
                catch { }
            }
        }

        /// <summary>
        /// Dumps PRAGMA table_info('Users') to a text file for quick debugging.
        /// </summary>
        private void DumpUsersTableInfo(string dbPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath)) return;
                var connString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
                using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info('Users');";
                using var reader = cmd.ExecuteReader();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"PRAGMA table_info('Users') for DB: {dbPath}");
                while (reader.Read())
                {
                    var cid = reader.GetInt32(reader.GetOrdinal("cid"));
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    var type = reader.GetString(reader.GetOrdinal("type"));
                    var notnull = reader.GetInt32(reader.GetOrdinal("notnull"));
                    var dflt = reader.IsDBNull(reader.GetOrdinal("dflt_value")) ? "(null)" : reader.GetString(reader.GetOrdinal("dflt_value"));
                    var pk = reader.GetInt32(reader.GetOrdinal("pk"));

                    sb.AppendLine($"{cid}\t{name}\t{type}\tnotnull={notnull}\tdefault={dflt}\tpk={pk}");
                }

                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TableInfo_Users.txt"), sb.ToString());
            }
            catch (Exception ex)
            {
                try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DumpUsersTableInfo_error.txt"), ex.ToString()); }
                catch { }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try { _host?.Dispose(); } catch { }
            _host = null;
        }
    }
}

using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Data;
using WeldAdminPro.Core.Configuration;

namespace WeldAdminPro.UI
{
	public partial class App : Application
	{
		public static ApplicationDbContext DbContext { get; private set; } = null!;
		public static ExecutiveSeverityOptions ExecutiveSeverityOptions { get; private set; } = new();

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// ==========================================
			// 1️⃣ Load Executive Severity Configuration
			// ==========================================
			try
			{
				var configPath = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					"appsettings.json");

				if (File.Exists(configPath))
				{
					var json = File.ReadAllText(configPath);

					using var document = JsonDocument.Parse(json);

					if (document.RootElement.TryGetProperty("ExecutiveSeverity", out var section))
					{
						var options = section.Deserialize<ExecutiveSeverityOptions>();

						if (options != null)
							ExecutiveSeverityOptions = options;
					}
				}
			}
			catch
			{
				// Fallback to defaults automatically
				ExecutiveSeverityOptions = new ExecutiveSeverityOptions();
			}

			// ==========================================
			// 2️⃣ Create EF DbContext
			// ==========================================
			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite($"Data Source={DatabasePath.Get()}")
				.Options;

			DbContext = new ApplicationDbContext(optionsBuilder);

			// ==========================================
			// 3️⃣ Global exception handler
			// ==========================================
			DispatcherUnhandledException += (sender, args) =>
			{
				MessageBox.Show(
					args.Exception.ToString(),
					"Unhandled Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);

				args.Handled = true;
			};
		}
	}
}
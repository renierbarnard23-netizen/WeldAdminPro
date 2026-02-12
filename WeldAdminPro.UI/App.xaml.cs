using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Data;

namespace WeldAdminPro.UI
{
	public partial class App : Application
	{
		public static ApplicationDbContext DbContext { get; private set; } = null!;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// ==========================================
			// 1️⃣ Create EF DbContext (Users / Auth)
			// ==========================================
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite($"Data Source={DatabasePath.Get()}")
				.Options;

			DbContext = new ApplicationDbContext(options);

			// ==========================================
			// 2️⃣ Global exception handler
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

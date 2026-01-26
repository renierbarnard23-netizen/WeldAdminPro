using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI
{
	public partial class App : Application
	{
		public static ApplicationDbContext DbContext { get; private set; } = null!;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// ==========================================
			// 1️⃣ Detect duplicate ItemCodes (SAFE GUARD)
			// ==========================================
			try
			{
				var stockRepo = new StockRepository();
				stockRepo.GetAll(); // triggers duplicate detection
			}
			catch (InvalidOperationException)
			{
				var fixWindow = new FixDuplicateItemCodesWindow
				{
					Owner = Current.MainWindow
				};

				fixWindow.ShowDialog();
			}

			// ==========================================
			// 2️⃣ Create EF DbContext (Users / Auth)
			// ==========================================
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite($"Data Source={DatabasePath.Get()}")
				.Options;

			DbContext = new ApplicationDbContext(options);

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

using System.Windows;
using WeldAdminPro.Data;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			DatabaseInitializer.Initialize();

            base.OnStartup(e);

			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var loginWindow = new LoginWindow();
			var result = loginWindow.ShowDialog();

			if (result == true)
			{
				var mainWindow = new MainWindow();

				MainWindow = mainWindow;
				mainWindow.Show();
				mainWindow.Activate();

				ShutdownMode = ShutdownMode.OnMainWindowClose;
			}
			else
			{
				Shutdown();
			}
		}
	}
	}

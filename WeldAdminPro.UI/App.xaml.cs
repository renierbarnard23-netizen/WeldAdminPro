using QuestPDF.Infrastructure;
using System;
using System.Windows;
using System.Windows.Threading;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.Converters;
using WeldAdminPro.UI.Views;




namespace WeldAdminPro.UI
{
    public partial class App : Application
    {        
        // =====================================
        // GLOBAL SERVICES
        // =====================================

        public static IProjectContextService
            ProjectContextService
                = new ProjectContextService();

        public static IWeldService
            WeldService
                = new WeldService(
                    new WeldRepository(
                        DatabasePath.GetConnectionString()));

        // =====================================
        // APPLICATION CONSTRUCTOR
        // =====================================

        public App()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }

        // =====================================
        // APPLICATION STARTUP
        // =====================================

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            ErrorLoggingService.Log(
                new Exception(
                    "WeldAdmin Pro Started"));

            // =====================================
            // GLOBAL EXCEPTION HANDLING
            // =====================================

            DispatcherUnhandledException +=
                App_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException;

            // =====================================
            // DATABASE INITIALIZATION
            // =====================================

            try
            {
                DatabaseInitializer.Initialize();

                var databasePath =
                    DatabasePath.Get();

                var migrationService =
                    new DatabaseMigrationService(
                        $"Data Source={databasePath}");

                migrationService.ApplyMigrations();

                var backupService =
                    new DatabaseBackupService(
                        databasePath);

                try
                {
                    backupService.CreateBackup();
                }
                catch (Exception ex)
                {
                    ErrorLoggingService.Log(ex);
                }
            }
            catch (Exception ex)
            {
                ErrorLoggingService.Log(ex);

                MessageBox.Show(
                    "WeldAdmin Pro was unable to initialize the database.\n\n" +
                    "Please see the log files for details.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }


            // =====================================
            // LOGIN FLOW
            // =====================================

            ShutdownMode =
                ShutdownMode.OnExplicitShutdown;

            var loginWindow =
                new LoginWindow();

            var result =
                loginWindow.ShowDialog();

            if (result == true)
            {
                var mainWindow =
                    new MainWindow();

                MainWindow =
                    mainWindow;

                mainWindow.Show();

                mainWindow.Activate();

                ShutdownMode =
                    ShutdownMode.OnMainWindowClose;
            }
            else
            {
                Shutdown();
            }
        }

        // =====================================
        // UI THREAD EXCEPTIONS
        // =====================================

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLoggingService.Log(
                e.Exception);

            MessageBox.Show(
                "An unexpected error occurred.\n\n" +
                "The error has been logged automatically.",
                "Application Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

            e.Handled = true;
        }

        // =====================================
        // NON-UI THREAD EXCEPTIONS
        // =====================================

        private void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ErrorLoggingService.Log(ex);
            }
        }
    }
}

using System;
using System.IO;

namespace WeldAdminPro.Data.Services
{
    public static class ErrorLoggingService
    {
        public static void Log(Exception exception)
        {
            try
            {
                var logFolder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.ApplicationData),
                        "WeldAdminPro",
                        "Logs");

                Directory.CreateDirectory(logFolder);

                var logPath =
                    Path.Combine(
                        logFolder,
                        $"{DateTime.Now:yyyy-MM-dd}.log");

                var message =
$@"==================================================
DATE:
{DateTime.Now:yyyy-MM-dd HH:mm:ss}

MACHINE:
{Environment.MachineName}

USER:
{Environment.UserName}

OS:
{Environment.OSVersion}

MESSAGE:
{exception.Message}

STACK TRACE:
{exception.StackTrace}

INNER EXCEPTION:
{exception.InnerException?.ToString() ?? "None"}

==================================================

";

                File.AppendAllText(
                    logPath,
                    message);
            }
            catch
            {
                // Never allow logging failures
                // to crash the application.
            }
        }
    }
}
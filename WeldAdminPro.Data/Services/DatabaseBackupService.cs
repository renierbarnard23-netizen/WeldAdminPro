using System;
using System.IO;

namespace WeldAdminPro.Data.Services
{
    public class DatabaseBackupService
    {
        private readonly string _databasePath;

        public DatabaseBackupService(
            string databasePath)
        {
            _databasePath =
                databasePath;
        }

        public void CreateBackup()
        {
            if (!File.Exists(_databasePath))
            {
                return;
            }

            var backupFolder =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Backups");

            Directory.CreateDirectory(
                backupFolder);

            var fileName =
                $"WeldAdminPro_" +
                $"{DateTime.Now:yyyyMMdd_HHmmss}.db";

            var backupPath =
                Path.Combine(
                    backupFolder,
                    fileName);

            File.Copy(
                _databasePath,
                backupPath,
                true);
        }
    }
}

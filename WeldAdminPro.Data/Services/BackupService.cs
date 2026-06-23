using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
    public class BackupService
    {
        private readonly string _databasePath;
        private readonly string _backupFolder;

        public BackupService()
        {
            _databasePath =
                DatabasePath.Get();

            _backupFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    "WeldAdminPro",
                    "Backups");

            Directory.CreateDirectory(
                _backupFolder);
        }

        // ==========================================
        // CREATE BACKUP
        // ==========================================
        public string CreateBackup()
        {
            if (!File.Exists(_databasePath))
            {
                throw new FileNotFoundException(
                    "Database file not found.",
                    _databasePath);
            }

            var fileName =
                $"WeldAdminPro_{DateTime.Now:yyyy-MM-dd_HHmmss}.db";

            var destination =
                Path.Combine(
                    _backupFolder,
                    fileName);

            File.Copy(
                _databasePath,
                destination,
                false);

            return destination;
        }

        // ==========================================
        // GET BACKUPS
        // ==========================================
        public List<BackupInfo> GetBackups()
        {
            if (!Directory.Exists(
                    _backupFolder))
            {
                return new List<BackupInfo>();
            }

            return Directory
                .GetFiles(
                    _backupFolder,
                    "*.db")
                .Select(x =>
                {
                    var file =
                        new FileInfo(x);

                    return new BackupInfo
                    {
                        FileName =
                            file.Name,

                        FullPath =
                            file.FullName,

                        CreatedOn =
                            file.CreationTime,

                        FileSizeBytes =
                            file.Length
                    };
                })
                .OrderByDescending(
                    x => x.CreatedOn)
                .ToList();
        }

        // ==========================================
        // DELETE BACKUP
        // ==========================================
        public void DeleteBackup(
            string fullPath)
        {
            if (string.IsNullOrWhiteSpace(
                    fullPath))
            {
                return;
            }

            if (File.Exists(
                    fullPath))
            {
                File.Delete(
                    fullPath);
            }
        }

        // ==========================================
        // RESTORE BACKUP
        // ==========================================
        public void RestoreBackup(
            string backupPath)
        {
            if (!File.Exists(
                    backupPath))
            {
                throw new FileNotFoundException(
                    "Backup file not found.",
                    backupPath);
            }

            CreateBackup();

            File.Copy(
                backupPath,
                _databasePath,
                true);
        }

        // ==========================================
        // KEEP LAST 20 BACKUPS
        // ==========================================
        public void CleanupOldBackups(
            int keepCount = 20)
        {
            var backups =
                GetBackups();

            if (backups.Count <= keepCount)
            {
                return;
            }

            foreach (var backup in
                     backups.Skip(
                         keepCount))
            {
                DeleteBackup(
                    backup.FullPath);
            }
        }

        // ==========================================
        // AUTOMATIC BACKUP
        // ==========================================
        public void CreateAutomaticBackup()
        {
            CreateBackup();
            CleanupOldBackups();
        }
    }
}
using System;

namespace WeldAdminPro.Core.Models
{
    public class BackupInfo
    {
        /// <summary>
        /// File name only.
        /// Example:
        /// WeldAdminPro_2026-06-22_091500.db
        /// </summary>
        public string FileName { get; set; }
            = string.Empty;

        /// <summary>
        /// Full path to backup file.
        /// </summary>
        public string FullPath { get; set; }
            = string.Empty;

        /// <summary>
        /// Date and time backup was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Size of backup file in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// File size in MB.
        /// Useful for UI display.
        /// </summary>
        public double FileSizeMb
        {
            get
            {
                return Math.Round(
                    FileSizeBytes /
                    1024d /
                    1024d,
                    2);
            }
        }
    }
}
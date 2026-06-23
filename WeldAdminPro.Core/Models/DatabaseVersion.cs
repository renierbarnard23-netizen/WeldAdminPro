using System;

namespace WeldAdminPro.Core.Models
{
    public class DatabaseVersion
    {
        public int Id { get; set; }

        public int SchemaVersion { get; set; }

        public string BuildVersion { get; set; }
            = "";

        public DateTime AppliedDate { get; set; }

        public string Notes { get; set; }
            = "";
    }
}
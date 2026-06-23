using System;

namespace WeldAdminPro.Core.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
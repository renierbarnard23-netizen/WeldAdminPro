using System;

namespace WeldAdminPro.Core.Models
{
    public class AuditTrailEntry
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public DateTime Timestamp { get; set; }
            = DateTime.Now;

        public string UserName { get; set; }
            = "";

        public string Module { get; set; }
            = "";

        public string Action { get; set; }
            = "";

        public string EntityType { get; set; }
            = "";

        public string EntityId { get; set; }
            = "";

        public string Details { get; set; }
            = "";
    }
}

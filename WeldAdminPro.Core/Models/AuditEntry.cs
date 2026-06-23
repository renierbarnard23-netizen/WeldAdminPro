using System;

namespace WeldAdminPro.Core.Models
{
	public class AuditEntry
	{
		public Guid Id { get; set; }
		public string ActionType { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? EntityId { get; set; }
		public string? Username { get; set; }
		public string? MachineName { get; set; }
		public string Severity { get; set; } = "Info";
		public DateTime Timestamp { get; set; }
	}
}
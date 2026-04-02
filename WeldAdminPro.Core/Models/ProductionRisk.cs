using System;

namespace WeldAdminPro.Core.Models
{
	public class ProductionRisk
	{
		public Guid WorkOrderId { get; set; }
		public string WorkOrderNumber { get; set; } = "";

		public RiskType RiskType { get; set; }

		public string Issue { get; set; } = "";
		public string Action { get; set; } = "";

		public DateTime? Deadline { get; set; }
	}
}
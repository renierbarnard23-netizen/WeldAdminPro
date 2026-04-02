using System;

namespace WeldAdminPro.Core.Models
{
	public class DeadlineRisk
	{
		public Guid WorkOrderId { get; set; }
		public string WorkOrderNumber { get; set; } = "";

		public DateTime Deadline { get; set; }

		public double RemainingHours { get; set; }
		public double AvailableHours { get; set; }

		public bool IsAtRisk { get; set; }
		public double DelayHours { get; set; }
	}
}
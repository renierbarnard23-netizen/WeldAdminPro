using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class DeadlineRisk
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime StartDate { get; set; }

		public DateTime DueDate { get; set; }

		public double EstimatedHours { get; set; }

		public double AvailableHours { get; set; }

		public string RiskLevel { get; set; } = "";

		public double HoursShortage { get; set; }
	}
}
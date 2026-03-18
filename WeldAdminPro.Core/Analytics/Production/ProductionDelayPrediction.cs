using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionDelayPrediction
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime ScheduledEnd { get; set; }

		public DateTime DueDate { get; set; }

		public double DaysLate { get; set; }

		public string RiskLevel { get; set; } = "";
		public int DelayDays { get; set; }
	}
}
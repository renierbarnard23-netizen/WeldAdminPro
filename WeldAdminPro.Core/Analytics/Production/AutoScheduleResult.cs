using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class AutoScheduleResult
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime OriginalDate { get; set; }

		public DateTime NewDate { get; set; }

		public double HoursMoved { get; set; }
	}
}
using System;

namespace WeldAdminPro.Core.Models
{
	public class ProductionScheduleItem
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public double DurationDays =>
			(EndDate - StartDate).TotalDays;
	}
}
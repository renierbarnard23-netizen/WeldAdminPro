using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionQueueItem
	{
		public Guid Id { get; set; }

		public string WorkOrderNumber { get; set; } = "";

		public int Priority { get; set; }

		public string Status { get; set; } = "";

		public string StartDate { get; set; } = "";

		public string DueDate { get; set; } = "";
		public bool IsTopPriority { get; set; }
	}
}
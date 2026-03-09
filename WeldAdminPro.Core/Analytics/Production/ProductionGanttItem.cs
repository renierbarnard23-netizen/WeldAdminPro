namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionGanttItem
	{
		public string WorkOrderNumber { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime DueDate { get; set; }

		public string Status { get; set; }

		public double StartOffset { get; set; }

		public double Duration { get; set; }
	}
}
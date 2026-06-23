namespace WeldAdminPro.Core.Analytics.Production
{
	public class SchedulerDebugItem
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public double Hours { get; set; }
	}
}
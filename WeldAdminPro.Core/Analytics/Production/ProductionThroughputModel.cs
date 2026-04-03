namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionThroughputModel
	{
		public int CompletedToday { get; set; }

		public double AvgDurationHours { get; set; }

		public int ActiveWorkOrders { get; set; }

		public double CompletionRate { get; set; }
	}
}
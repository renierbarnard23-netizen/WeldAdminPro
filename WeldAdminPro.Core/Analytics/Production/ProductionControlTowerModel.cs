namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionControlTowerModel
	{
		public int ReadyOrders { get; set; }

		public int RunningOrders { get; set; }

		public int BlockedOrders { get; set; }

		public int CompletedToday { get; set; }

		public double CapacityLoad { get; set; }

		public int DeadlineRisks { get; set; }
	}
}
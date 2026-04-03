namespace WeldAdminPro.Core.Analytics.Executive
{
	public class InventoryRiskSummary
	{
		public int NegativeStockItems { get; set; }

		public int DeadStockItems { get; set; }

		public int ConsumptionSpikes { get; set; }

		public int CriticalInventoryItems { get; set; }

		public string OverallRiskLevel { get; set; } = "LOW";

		public int HealthScore { get; set; }
	}
}
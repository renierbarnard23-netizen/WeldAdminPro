namespace WeldAdminPro.Core.Configuration
{
	public class ExecutiveSeverityOptions
	{
		// -----------------------------
		// Health Thresholds
		// -----------------------------
		public int HealthStableThreshold { get; set; } = 80;
		public int HealthModerateThreshold { get; set; } = 60;
		public int HealthCriticalItemsEscalation { get; set; } = 3;
		public double DeadStockHighThresholdPercent { get; set; } = 10;
		public double DeadStockModerateThresholdPercent { get; set; } = 5;

		// -----------------------------
		// Capital Exposure Thresholds
		// -----------------------------
		public double CapitalAHighThresholdPercent { get; set; } = 75;
		public double CapitalAModerateThresholdPercent { get; set; } = 60;
		public int HighRiskHighValueEscalation { get; set; } = 3;

		// -----------------------------
		// Reorder / Stockout Thresholds
		// -----------------------------
		public int StockoutHighThresholdCount { get; set; } = 5;
		public int ReorderHighThresholdCount { get; set; } = 3;

		// -----------------------------
		// Version Tag (Audit Support)
		// -----------------------------
		public string RulesVersion { get; set; } = "1.0";
	}
}
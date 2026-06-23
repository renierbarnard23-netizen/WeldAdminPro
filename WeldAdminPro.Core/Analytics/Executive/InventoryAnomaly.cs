namespace WeldAdminPro.Core.Analytics.Executive
{
	public class InventoryAnomaly
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public double CurrentConsumption { get; set; }

		public double HistoricalAverage { get; set; }

		public double IncreasePercentage { get; set; }

		public string Severity { get; set; } = "Low";
	}
}
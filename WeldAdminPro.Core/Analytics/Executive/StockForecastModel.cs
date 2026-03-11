namespace WeldAdminPro.Core.Analytics.Executive
{
	public class StockForecastModel
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public decimal CurrentStock { get; set; }

		public decimal AvgMonthlyUse { get; set; }

		public decimal DaysRemaining { get; set; }

		public string Status { get; set; } = "";
	}
}
namespace WeldAdminPro.Core.Analytics.Executive
{
	public class MaterialTrendModel
	{
		public string ItemCode { get; set; }

		public string Description { get; set; }

		public decimal LastMonthCost { get; set; }

		public decimal ThisMonthCost { get; set; }

		public decimal PercentChange { get; set; }
	}
}
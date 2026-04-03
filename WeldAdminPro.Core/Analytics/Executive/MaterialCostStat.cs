namespace WeldAdminPro.Core.Analytics.Executive
{
	public class MaterialCostStat
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public decimal TotalCost { get; set; }

		public int TotalQuantity { get; set; }
	}
}
namespace WeldAdminPro.Core.Analytics.Production
{
	public class WorkOrderMaterialPlan
	{
		public string WorkOrderNumber { get; set; } = "";
		public string ItemCode { get; set; } = "";
		public string Description { get; set; } = "";

		public int RequiredQuantity { get; set; }
		public int StockAvailable { get; set; }

		public int Shortage =>
			RequiredQuantity > StockAvailable
			? RequiredQuantity - StockAvailable
			: 0;
	}
}
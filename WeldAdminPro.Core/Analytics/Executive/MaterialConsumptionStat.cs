namespace WeldAdminPro.Core.Analytics.Executive
{
	public class MaterialConsumptionStat
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int TotalConsumed { get; set; }

		public decimal EstimatedCost { get; set; }
	}
}
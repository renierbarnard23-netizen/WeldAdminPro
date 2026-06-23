namespace WeldAdminPro.Core.Models
{
	public class MaterialCostDriver
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int UnitsConsumed { get; set; }

		public decimal TotalCost { get; set; }
	}
}
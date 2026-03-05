namespace WeldAdminPro.Core.Models
{
	public class ProjectMaterialBreakdown
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int QuantityUsed { get; set; }

		public decimal MaterialCost { get; set; }
	}
}
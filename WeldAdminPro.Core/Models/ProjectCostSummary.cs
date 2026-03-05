namespace WeldAdminPro.Core.Models
{
	public class ProjectCostSummary
	{
		public Guid ProjectId { get; set; }

		public string ProjectName { get; set; } = "";

		public decimal TotalMaterialCost { get; set; }

		public int TotalUnitsConsumed { get; set; }
	}
}
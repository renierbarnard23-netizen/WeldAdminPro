namespace WeldAdminPro.Core.Models
{
	public class ProjectProfitability
	{
		public Guid ProjectId { get; set; }

		public string ProjectName { get; set; } = "";

		public decimal Revenue { get; set; }

		public decimal MaterialCost { get; set; }

		public decimal LabourCost { get; set; }

		public decimal Profit =>
			Revenue - MaterialCost - LabourCost;
	}
}
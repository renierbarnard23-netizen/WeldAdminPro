namespace WeldAdminPro.Core.Analytics.Executive
{
	public class ProjectRiskModel
	{
		public string ProjectId { get; set; }

		public string ProjectName { get; set; }

		public decimal Budget { get; set; }

		public decimal MaterialCost { get; set; }

		public decimal BudgetUsedPercent { get; set; }

		public string RiskLevel { get; set; }
	}
}
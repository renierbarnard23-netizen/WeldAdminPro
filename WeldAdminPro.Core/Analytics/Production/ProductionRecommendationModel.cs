namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionRecommendationModel
	{
		public string WorkOrderNumber { get; set; } = "";

		public string Recommendation { get; set; } = "";

		public string Reason { get; set; } = "";

		public int Score { get; set; }
	}
}
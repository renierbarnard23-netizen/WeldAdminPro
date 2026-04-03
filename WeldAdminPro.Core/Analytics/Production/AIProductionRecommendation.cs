namespace WeldAdminPro.Core.Analytics.Production
{
	public class AIProductionRecommendation
	{
		public string WorkOrderNumber { get; set; } = "";
		public string Due { get; set; } = "";
		public string Materials { get; set; } = "";
		public double PriorityScore { get; set; }
		public string Recommendation { get; set; } = "";
		public string Explanation { get; set; } = "";
	}
}
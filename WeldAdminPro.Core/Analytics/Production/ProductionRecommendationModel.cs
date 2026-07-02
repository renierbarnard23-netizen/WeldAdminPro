namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionRecommendationModel
	{
		public string WorkOrderNumber { get; set; } = "";
		public string Recommendation { get; set; } = "";
		public string Explanation { get; set; } = "";
		public int Score { get; set; }

        public string Category { get; set; } = "";

        public bool RequiresAction { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
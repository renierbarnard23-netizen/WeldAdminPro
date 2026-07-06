namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionAdvisorResult
	{
		public string WorkOrderNumber { get; set; } = "";
		public string Recommendation { get; set; } = "";
		public string Reason { get; set; } = "";

		// 🔥 ADD THIS LINE
		public string Explanation { get; set; } = "";

        public string RiskLevel { get; set; } = "";

        public bool ReadyToStart { get; set; }

        public bool MaterialShortage { get; set; }

        public bool ProductionBlocked { get; set; }

        public int CompletionPercent { get; set; }

        public double PriorityScore { get; set; }
    }
}
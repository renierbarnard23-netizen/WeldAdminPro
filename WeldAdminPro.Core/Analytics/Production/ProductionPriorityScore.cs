namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionPriorityScore
	{
		public string WorkOrderNumber { get; set; } = "";
		public double Score { get; set; }

		public bool MaterialsReady { get; set; }

		public double DueDateScore { get; set; }
		public double MaterialScore { get; set; }
		public double DurationScore { get; set; }

		public string Recommendation =>
			Score >= 75 ? "Start Immediately"
			: Score >= 50 ? "Queue Next"
			: "Delay / Resolve Issues";
	}
}
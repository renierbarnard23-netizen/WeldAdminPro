namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionAdvisorResult
	{
		public string WorkOrderNumber { get; set; } = "";
		public string Recommendation { get; set; } = "";
		public string Reason { get; set; } = "";

		// 🔥 ADD THIS LINE
		public string Explanation { get; set; } = "";
	}
}
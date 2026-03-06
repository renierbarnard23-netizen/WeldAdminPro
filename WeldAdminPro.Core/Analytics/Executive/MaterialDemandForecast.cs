namespace WeldAdminPro.Core.Analytics.Executive
{
	public class MaterialDemandForecast
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int CurrentStock { get; set; }

		public double AverageDailyConsumption { get; set; }

		public int DaysRemaining { get; set; }

		public string Status { get; set; } = "OK";

		public int PriorityScore { get; set; }
	}
}
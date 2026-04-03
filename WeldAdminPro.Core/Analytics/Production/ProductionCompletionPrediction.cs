namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionCompletionPrediction
	{
		public string WorkOrderNumber { get; set; } = "";
		public DateTime PredictedStart { get; set; }
		public DateTime PredictedEnd { get; set; }
		public bool IsLate { get; set; }
		public int DaysLate { get; set; }
	}
}
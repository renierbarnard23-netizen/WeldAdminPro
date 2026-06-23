namespace WeldAdminPro.Core.Models
{
	public class ProductionPriorityExplanation
	{
		public string WorkOrderNumber { get; set; } = "";
		public int TotalScore { get; set; }

		public int DueDateScore { get; set; }
		public int MaterialScore { get; set; }
		public int DurationScore { get; set; }

		public string Explanation =>
			$"Due Date: +{DueDateScore}, Materials: +{MaterialScore}, Duration: +{DurationScore}";
	}
}
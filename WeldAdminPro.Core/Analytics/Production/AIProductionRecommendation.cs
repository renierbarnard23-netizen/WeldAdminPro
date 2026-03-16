public class AIProductionRecommendation
{
	public string WorkOrderNumber { get; set; } = "";

	public string Due { get; set; } = "";

	public string Materials { get; set; } = "";

	public double PriorityScore { get; set; }

	public string Recommendation { get; set; } = "";
}
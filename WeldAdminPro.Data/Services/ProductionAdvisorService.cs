using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services
{
	public class ProductionAdvisorService
	{
		private readonly ProductionAIPlannerService _planner;
		private readonly ProductionBottleneckDetectionService _bottlenecks;

		public ProductionAdvisorService()
		{
			_planner = new ProductionAIPlannerService();
			_bottlenecks = new ProductionBottleneckDetectionService();
		}

		public ProductionAdvisorResult GetNextBestAction()
		{
			var recommendations = _planner.GetRecommendations();
			var bottlenecks = _bottlenecks.DetectBottlenecks();

			var best = recommendations
				.Where(r => r.Materials == "Ready")
				.OrderByDescending(r => r.PriorityScore)
				.FirstOrDefault();

			if (best == null)
			{
				return new ProductionAdvisorResult
				{
					WorkOrderNumber = "-",
					Recommendation = "No work can start",
					Reason = "All jobs blocked by material shortages"
				};
			}

			var relatedRisk = bottlenecks
				.FirstOrDefault(b => b.WorkOrderNumber == best.WorkOrderNumber);

			string reason = best.Explanation;

			if (relatedRisk != null)
			{
				reason += $" | Risk: {relatedRisk.BottleneckType}";
			}

			return new ProductionAdvisorResult
			{
				WorkOrderNumber = best.WorkOrderNumber,
				Recommendation = "Start Next",
				Reason = reason
			};
		}
	}
}
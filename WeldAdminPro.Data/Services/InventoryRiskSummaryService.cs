using WeldAdminPro.Core.Analytics.Executive;

namespace WeldAdminPro.Data.Services
{
	public class InventoryRiskSummaryService
	{
		private readonly InventoryRiskService _riskService;

		public InventoryRiskSummaryService()
		{
			_riskService = new InventoryRiskService();
		}

		public InventoryRiskSummary BuildSummary()
		{
			var negative = _riskService.GetNegativeStockItems().Count;
			var dead = _riskService.GetDeadStockItems().Count;
			var spikes = _riskService.DetectConsumptionSpikes().Count;
			var critical = _riskService.DetectCriticalStockConcentration().Count;

			var summary = new InventoryRiskSummary
			{
				NegativeStockItems = negative,
				DeadStockItems = dead,
				ConsumptionSpikes = spikes,
				CriticalInventoryItems = critical
			};

			int score = negative + dead + spikes + critical;

			if (score == 0)
				summary.OverallRiskLevel = "LOW";
			else if (score <= 3)
				summary.OverallRiskLevel = "MEDIUM";
			else
				summary.OverallRiskLevel = "HIGH";

			// Health score calculation
			int penalty = (negative * 20) + (dead * 5) + (spikes * 10) + (critical * 15);

			int health = 100 - penalty;

			if (health < 0)
				health = 0;

			summary.HealthScore = health;

			return summary;
		}
	}
}
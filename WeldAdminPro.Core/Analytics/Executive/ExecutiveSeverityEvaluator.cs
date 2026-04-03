using WeldAdminPro.Core.Configuration;
using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Analytics.Executive
{
	public class ExecutiveSeverityEvaluator
	{
		private readonly ExecutiveSeverityOptions _options;

		public ExecutiveSeverityEvaluator(ExecutiveSeverityOptions? options = null)
		{
			_options = options ?? new ExecutiveSeverityOptions();
		}

		// =====================================================
		// 1️⃣ Overall Inventory Health
		// =====================================================
		public ExecutiveSeverityLevel EvaluateHealth(
			int healthScore,
			int criticalItemsCount,
			double deadStockPercent)
		{
			// Escalation rules first (fail-fast)
			if (healthScore < _options.HealthModerateThreshold ||
				criticalItemsCount >= _options.HealthCriticalItemsEscalation ||
				deadStockPercent > _options.DeadStockHighThresholdPercent)
			{
				return ExecutiveSeverityLevel.AtRisk;
			}

			if (healthScore < _options.HealthStableThreshold ||
				deadStockPercent > _options.DeadStockModerateThresholdPercent ||
				criticalItemsCount > 0)
			{
				return ExecutiveSeverityLevel.Moderate;
			}

			return ExecutiveSeverityLevel.Stable;
		}

		// =====================================================
		// 2️⃣ Capital Exposure & Concentration
		// =====================================================
		public ExecutiveSeverityLevel EvaluateCapital(
			double percentInAClass,
			int highRiskHighValueOverlapCount,
			double deadStockPercent)
		{
			if (percentInAClass > _options.CapitalAHighThresholdPercent ||
				highRiskHighValueOverlapCount >= _options.HighRiskHighValueEscalation ||
				deadStockPercent > _options.DeadStockHighThresholdPercent)
			{
				return ExecutiveSeverityLevel.AtRisk;
			}

			if (percentInAClass >= _options.CapitalAModerateThresholdPercent ||
				highRiskHighValueOverlapCount > 0 ||
				deadStockPercent > _options.DeadStockModerateThresholdPercent)
			{
				return ExecutiveSeverityLevel.Moderate;
			}

			return ExecutiveSeverityLevel.Stable;
		}

		// =====================================================
		// 3️⃣ Reorder & Stockout Pressure
		// =====================================================
		public ExecutiveSeverityLevel EvaluateReorder(
			int itemsBelow30Days,
			int criticalAItems,
			int reorderRequiredCount)
		{
			if (itemsBelow30Days > _options.StockoutHighThresholdCount ||
				criticalAItems >= 1 ||
				reorderRequiredCount > _options.ReorderHighThresholdCount)
			{
				return ExecutiveSeverityLevel.AtRisk;
			}

			if (itemsBelow30Days > 0 ||
				reorderRequiredCount > 0)
			{
				return ExecutiveSeverityLevel.Moderate;
			}

			return ExecutiveSeverityLevel.Stable;
		}
	}
}
using System.Text;
using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Analytics.Executive
{
	public class ExecutiveSummaryBuilder
	{
		public ExecutiveSummaryBlock BuildHealthBlock(
			ExecutiveSeverityLevel severity,
			int healthScore,
			int criticalItemsCount,
			double deadStockPercent)
		{
			var paragraph = severity switch
			{
				ExecutiveSeverityLevel.Stable =>
					"Inventory health is stable with controlled operational exposure.",

				ExecutiveSeverityLevel.Moderate =>
					"Inventory health is moderate with areas requiring monitoring to prevent operational risk.",

				ExecutiveSeverityLevel.AtRisk =>
					"Inventory health is under pressure with elevated operational risk requiring corrective action.",

				_ => string.Empty
			};

			return new ExecutiveSummaryBlock
			{
				Title = "Overall Inventory Health",
				Severity = severity,
				Paragraph = paragraph
			};
		}

		public ExecutiveSummaryBlock BuildCapitalBlock(
			ExecutiveSeverityLevel severity,
			double percentInAClass,
			int overlapCount,
			double deadStockPercent)
		{
			var paragraph = severity switch
			{
				ExecutiveSeverityLevel.Stable =>
					"Capital exposure is balanced with no significant concentration risk identified.",

				ExecutiveSeverityLevel.Moderate =>
					"Capital exposure is elevated due to concentration within high-value items, requiring monitoring.",

				ExecutiveSeverityLevel.AtRisk =>
					"Capital concentration risk is high due to reliance on high-value items, increasing financial sensitivity.",

				_ => string.Empty
			};

			return new ExecutiveSummaryBlock
			{
				Title = "Capital Exposure & Concentration",
				Severity = severity,
				Paragraph = paragraph
			};
		}
		
		public ExecutiveSummaryBlock BuildReorderBlock(
			ExecutiveSeverityLevel severity,
			int itemsBelow30Days,
			int criticalAItems,
			int reorderRequiredCount)
		{
			var paragraph = severity switch
			{
				ExecutiveSeverityLevel.Stable =>
					"Reorder pressure is controlled with no immediate stockout risk identified.",

				ExecutiveSeverityLevel.Moderate =>
					"Reorder pressure is increasing due to items approaching short-term replenishment thresholds.",

				ExecutiveSeverityLevel.AtRisk =>
					"Immediate replenishment attention is required due to elevated short-term stockout risk.",

				_ => string.Empty
			};


			return new ExecutiveSummaryBlock
			{
				Title = "Reorder & Stockout Pressure",
				Severity = severity,
				Paragraph = paragraph
			};

		}
	}
}
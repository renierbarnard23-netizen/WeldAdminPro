using System;

namespace WeldAdminPro.Core.Models
{
	public class SmartStockInsight
	{
		public Guid StockItemId { get; set; }

		public decimal AverageDailyUsage { get; set; }

		public int DaysRemaining { get; set; }

		public int SuggestedReorderPoint { get; set; }

		public int SuggestedReorderQuantity { get; set; }

		public SmartStockRiskLevel RiskLevel { get; set; }
	}

	public enum SmartStockRiskLevel
	{
		Normal,
		Warning,
		Critical
	}
}

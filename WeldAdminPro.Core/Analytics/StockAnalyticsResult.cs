using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Analytics
{
	public class StockAnalyticsResult
	{
		// =============================
		// High-Level Financial Metrics
		// =============================

		public decimal TotalInventoryValue { get; set; }
		public double CapitalLockedPercentage { get; set; }

		// =============================
		// ABC Distribution
		// =============================

		public int AItemCount { get; set; }
		public int BItemCount { get; set; }
		public int CItemCount { get; set; }

		// =============================
		// Executive Severity Metrics
		// =============================

		public int InventoryHealthScore { get; set; }

		public int CriticalItemCount { get; set; }

		public double DeadStockPercentage { get; set; }

		public int HighRiskHighValueOverlapCount { get; set; }

		public int ItemsBelow30DaysCount { get; set; }

		public int CriticalAItemCount { get; set; }

		public int ReorderRequiredCount { get; set; }

		// =============================
		// Item-Level Summaries
		// =============================

		public List<ItemMovementSummary> ItemSummaries { get; set; }
	= new List<ItemMovementSummary>();
	}
}
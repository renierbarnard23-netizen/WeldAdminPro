using System;
using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Models
{
	public class ItemMovementSummary
	{
		public Guid StockItemId { get; set; }

		// ==============================
		// IDENTIFICATION
		// ==============================

		public string ItemCode { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		// ==============================
		// INVENTORY CLASSIFICATION
		// ==============================

		public ItemInventoryCategory InventoryCategory { get; set; }

		public string InventoryCategoryDisplay =>
			InventoryCategory.ToString();


		// ==============================
		// HEALTH STATUS
		// ==============================

		public InventoryHealthStatus HealthStatus { get; set; }

		public string HealthStatusDisplay =>
			HealthStatus.ToString();

		// ==============================
		// MOVEMENT DATA
		// ==============================

		public int TotalIn { get; set; }
		public int TotalOut { get; set; }

		public int NetMovement => TotalIn - TotalOut;

		public decimal MovementValue { get; set; }

		// ==============================
		// STOCK POSITION
		// ==============================

		public decimal CurrentBalance { get; set; }
		public decimal CurrentStockValue { get; set; }

		// ==============================
		// INVENTORY TURNOVER INTELLIGENCE
		// ==============================

		public decimal AverageInventory { get; set; }
		public decimal TurnoverRate { get; set; }
		public decimal DaysInInventory { get; set; }

		// ==============================
		// USAGE & REORDER INTELLIGENCE
		// ==============================

		public decimal AverageDailyUsage { get; set; }
		public decimal DaysUntilStockout { get; set; }

		public int SuggestedReorderQuantity { get; set; }
		public decimal SuggestedOrderValue { get; set; }

		public DateTime? EstimatedStockoutDate { get; set; }

		public string ReorderRiskLevel { get; set; } = "-";
	}
}
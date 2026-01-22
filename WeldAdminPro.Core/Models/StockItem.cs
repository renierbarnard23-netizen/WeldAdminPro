using System;

namespace WeldAdminPro.Core.Models
{
	public class StockItem
	{
		public Guid Id { get; set; }

		public string ItemCode { get; set; } = "";
		public string Description { get; set; } = "";

		/// <summary>
		/// Current on-hand quantity.
		/// Updated only via stock transactions.
		/// </summary>
		public int Quantity { get; set; }

		public string Unit { get; set; } = "";
		public string Category { get; set; } = "Uncategorised";

		// =========================
		// PER-ITEM STOCK LEVELS
		// =========================

		/// <summary>
		/// Minimum acceptable stock level for THIS item.
		/// When Quantity <= MinLevel, item is considered low stock.
		/// Set to 0 to disable low-stock tracking for this item.
		/// </summary>
		public int MinLevel { get; set; } = 0;

		/// <summary>
		/// Desired maximum stock level for THIS item.
		/// Used to calculate suggested reorder quantity.
		/// Set to 0 to disable automatic suggestions.
		/// </summary>
		public int MaxLevel { get; set; } = 0;

		// =========================
		// DERIVED STATUS (READ-ONLY)
		// =========================

		/// <summary>
		/// True when stock is at or below the minimum level.
		/// Disabled when MinLevel is 0.
		/// </summary>
		public bool IsLowStock =>
			MinLevel > 0 &&
			Quantity > 0 &&
			Quantity <= MinLevel;

		/// <summary>
		/// True when no stock remains.
		/// </summary>
		public bool IsOutOfStock =>
			Quantity <= 0;

		/// <summary>
		/// Suggested reorder quantity based on MaxLevel.
		/// Returns 0 when reorder is not applicable.
		/// </summary>
		public int SuggestedReorderQuantity =>
			(MinLevel > 0 &&
			 MaxLevel > MinLevel &&
			 Quantity <= MinLevel)
				? Math.Max(0, MaxLevel - Quantity)
				: 0;
	}
}

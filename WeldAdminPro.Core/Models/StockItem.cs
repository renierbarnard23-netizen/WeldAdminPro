using System;

namespace WeldAdminPro.Core.Models
{
	public class StockItem
	{
		public Guid Id { get; set; }

		public string ItemCode { get; set; } = "";
		public string Description { get; set; } = "";

		// Quantity remains INT for now (matches existing logic)
		public int Quantity { get; set; }

		// Existing column – keep it
		public string Unit { get; set; } = "";

		// New columns (nullable, backward-safe)
		public decimal? MinLevel { get; set; }
		public decimal? MaxLevel { get; set; }

		// Required
		public string Category { get; set; } = "Uncategorised";

		// ❌ NOT persisted – derived values
		public bool IsLowStock =>
			MinLevel.HasValue && Quantity <= MinLevel.Value;

		public bool IsOutOfStock =>
			Quantity <= 0;
	}
}

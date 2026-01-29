using System;

namespace WeldAdminPro.Core.Models
{
	public class StockItem
	{
		public Guid Id { get; set; }

		public string ItemCode { get; set; } = "";
		public string Description { get; set; } = "";
		public int Quantity { get; set; }
		public string Unit { get; set; } = "";
		public decimal? MinLevel { get; set; }
		public decimal? MaxLevel { get; set; }
		public string Category { get; set; } = "Uncategorised";

		// 🔴 Out of stock = ZERO or less
		public bool IsOutOfStock => Quantity <= 0;

		// 🟡 Low stock = ABOVE zero AND below min
		public bool IsLowStock =>
			MinLevel.HasValue &&
			Quantity > 0 &&
			Quantity <= MinLevel.Value;

		// 🧠 Unified stock status (Phase 9.1)
		public StockStatus Status
		{
			get
			{
				if (IsOutOfStock)
					return StockStatus.Out;

				if (IsLowStock)
					return StockStatus.Low;

				return StockStatus.Normal;
			}
		}
	}
}

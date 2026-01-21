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
		public string Category { get; set; } = "Uncategorised";

		// =========================
		// Low stock settings
		// =========================

		/// <summary>
		/// Default low-stock threshold.
		/// Can be made configurable later.
		/// </summary>
		public int LowStockThreshold { get; set; } = 10;

		public bool IsLowStock =>
			Quantity > 0 && Quantity <= LowStockThreshold;

		public bool IsOutOfStock =>
			Quantity <= 0;
	}
}

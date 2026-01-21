using System;

namespace WeldAdminPro.Core.Models
{
	public class StockItem
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public string ItemCode { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public string Unit { get; set; } = string.Empty;

		// ✅ NEW
		public string Category { get; set; } = string.Empty;
	}
}

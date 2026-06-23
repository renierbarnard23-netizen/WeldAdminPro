using System;

namespace WeldAdminPro.Core.Models
{
	public class ProjectStockSummary
	{
		public Guid StockItemId { get; set; }

		public string ItemCode { get; set; } = "";
		public string Description { get; set; } = "";
		public string Unit { get; set; } = "";

		public decimal IssuedQuantity { get; set; }
		public decimal ReturnedQuantity { get; set; }

		public decimal NetQuantity => IssuedQuantity - ReturnedQuantity;

		// ✅ ADD THIS
		public bool HasNegativeNetUsage => NetQuantity < 0;
	}
}

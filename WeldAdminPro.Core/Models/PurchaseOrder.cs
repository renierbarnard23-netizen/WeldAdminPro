using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldAdminPro.Core.Models
{
	public class PurchaseOrder
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid ProjectId { get; set; }

		public int JobNumber { get; set; }

		public string PONumber { get; set; } = string.Empty;

		public string SupplierName { get; set; } = string.Empty;

		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

		public string Status { get; set; } = "Draft";

		// 🔧 Writable (repository assigns this)
		public decimal TotalAmount { get; set; }

		// Use List for repository compatibility
		public List<PurchaseOrderLine> Lines { get; set; } = new();

		// 🔧 Optional helper if you want automatic recalc
		public void RecalculateTotal()
		{
			TotalAmount = Lines.Sum(l => l.LineTotal);
		}
	}

	public class PurchaseOrderLine
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid PurchaseOrderId { get; set; }

		public Guid StockItemId { get; set; }

		public string ItemCode { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public decimal UnitCost { get; set; }

		// 🔧 Writable (repository assigns this)
		public decimal LineTotal { get; set; }

		// Optional helper
		public void RecalculateLineTotal()
		{
			LineTotal = Quantity * UnitCost;
		}
	}
}

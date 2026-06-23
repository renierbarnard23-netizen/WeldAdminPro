using System;

namespace WeldAdminPro.Core.Models
{
	public class ProjectStockAllocation
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid ProjectId { get; set; }
		public Guid StockItemId { get; set; }

		public int Quantity { get; set; }

		// Snapshot of cost at allocation time
		public decimal UnitCost { get; set; }

		public DateTime AllocationDate { get; set; } = DateTime.Now;

		public string Reference { get; set; } = string.Empty;

		// Optional display helpers
		public string? ProjectName { get; set; }
		public string? ItemCode { get; set; }
		public string? ItemDescription { get; set; }
	}
}

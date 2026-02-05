using System;

namespace WeldAdminPro.Core.Models
{
	public class ProjectStockUsage
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid ProjectId { get; set; }

		public Guid StockItemId { get; set; }

		public decimal Quantity { get; set; }

		public DateTime IssuedOn { get; set; } = DateTime.Now;

		public string IssuedBy { get; set; } = string.Empty;

		public string? Notes { get; set; }

		// ✅ SAFE DISPLAY NAME (USED BY UI)
		public string DisplayName =>
			string.IsNullOrWhiteSpace(Notes)
				? StockItemId.ToString()
				: Notes;
	}
}

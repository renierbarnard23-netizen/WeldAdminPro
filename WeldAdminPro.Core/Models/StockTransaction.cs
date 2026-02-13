using System;

namespace WeldAdminPro.Core.Models
{
	public class StockTransaction
	{
		public Guid Id { get; set; }

		public Guid StockItemId { get; set; }

		// ✅ NEW – optional project link
		public Guid? ProjectId { get; set; }

		public DateTime TransactionDate { get; set; }

		// Always stored positive
		public int Quantity { get; set; }

		// "IN" or "OUT"
		public string Type { get; set; } = string.Empty;

		public decimal UnitCost { get; set; }

		public string? Reference { get; set; }

		// Running balance after transaction
		public int BalanceAfter { get; set; }

		// For joined history display
		public string ItemCode { get; set; } = string.Empty;

		public string ItemDescription { get; set; } = string.Empty;
	}
}

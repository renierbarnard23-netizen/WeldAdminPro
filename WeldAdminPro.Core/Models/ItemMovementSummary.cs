using System;

namespace WeldAdminPro.Core.Models
{
	public class ItemMovementSummary
	{
		public Guid StockItemId { get; set; }

		public string ItemCode { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public int TotalIn { get; set; }
		public int TotalOut { get; set; }

		public int NetMovement => TotalIn - TotalOut;

		public decimal MovementValue { get; set; }

		public int CurrentBalance { get; set; }

		public decimal CurrentStockValue { get; set; }
	}
}

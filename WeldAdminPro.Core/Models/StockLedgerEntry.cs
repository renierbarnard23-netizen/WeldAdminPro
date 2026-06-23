using System;

namespace WeldAdminPro.Core.Models
{
	public class StockLedgerEntry
	{
		public DateTime TransactionDate { get; set; }

		public string Type { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public string? Reference { get; set; }

		public int CalculatedBalance { get; set; }

		public int StoredBalance { get; set; }

		public bool IsMismatch => CalculatedBalance != StoredBalance;
	}
}

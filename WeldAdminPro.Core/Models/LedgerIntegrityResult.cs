using System;

namespace WeldAdminPro.Core.Models
{
	public class LedgerIntegrityResult
	{
		public bool HasFailure { get; set; }

		public string Message { get; set; } = string.Empty;

		public int TransactionsChecked { get; set; }

		public int ErrorsDetected { get; set; }

		public Guid? FailedItemId { get; set; }

		public string FailedItemCode { get; set; } = string.Empty;
	}
}
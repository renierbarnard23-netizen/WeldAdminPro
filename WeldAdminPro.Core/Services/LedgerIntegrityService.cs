using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
	public class LedgerIntegrityService
	{
		public LedgerIntegrityResult ValidateLedger(IEnumerable<StockTransaction> transactions)
		{
			var ordered = transactions
				.OrderBy(t => t.TransactionDate)
				.ToList();

			decimal runningBalance = 0;
			int checkedCount = 0;

			foreach (var tx in ordered)
			{
				runningBalance += tx.QtyIn;
				runningBalance -= tx.QtyOut;

				checkedCount++;

				if (tx.BalanceAfter != runningBalance)
				{
					return new LedgerIntegrityResult
					{
						HasFailure = true,
						Message = "Ledger drift detected",
						TransactionsChecked = checkedCount,
						ErrorsDetected = 1,
						FailedItemId = tx.StockItemId
					};
				}
			}

			return new LedgerIntegrityResult
			{
				HasFailure = false,
				Message = "Ledger integrity verified",
				TransactionsChecked = checkedCount
			};
		}
	}
}
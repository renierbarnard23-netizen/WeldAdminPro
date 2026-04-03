using System;
using System.Linq;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class LedgerService
	{
		private readonly StockTransactionRepository _transactionRepo;

		public LedgerService()
		{
			_transactionRepo = new StockTransactionRepository();
		}

		// =========================================================
		// REBUILD LEDGER BALANCES
		// =========================================================

		public void RecalculateAllBalances()
		{
			var transactions = _transactionRepo.GetAllTransactions();

			var grouped = transactions
				.GroupBy(t => t.StockItemId);

			foreach (var group in grouped)
			{
				int running = 0;

				foreach (var tx in group.OrderBy(t => t.TransactionDate))
				{
					if (tx.Type == "IN" || tx.Type == "RET")
						running += tx.Quantity;

					else if (tx.Type == "OUT")
						running -= tx.Quantity;

					_transactionRepo.UpdateTransactionBalance(tx.Id, running);
				}
			}
		}

		// =========================================================
		// LEDGER INTEGRITY CHECK
		// =========================================================

		public bool VerifyLedgerIntegrity()
		{
			var transactions = _transactionRepo.GetAllTransactions();

			var grouped = transactions
				.GroupBy(t => t.StockItemId);

			foreach (var group in grouped)
			{
				int running = 0;

				foreach (var tx in group.OrderBy(t => t.TransactionDate))
				{
					if (tx.Type == "IN" || tx.Type == "RET")
						running += tx.Quantity;

					else if (tx.Type == "OUT")
						running -= tx.Quantity;

					if (tx.BalanceAfter != running)
						return false;
				}
			}

			return true;
		}

		// =========================================================
		// AUTO REPAIR
		// =========================================================

		public void RepairLedgerIfRequired()
		{
			if (!VerifyLedgerIntegrity())
			{
				RecalculateAllBalances();
			}
		}
	}
}
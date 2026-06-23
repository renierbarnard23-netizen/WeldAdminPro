using System.Linq;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class LedgerRepairService
	{
		private readonly StockRepository _repository;

		public LedgerRepairService()
		{
			_repository = new StockRepository();
		}

		public int RepairLedger()
		{
			var transactions = _repository
				.GetAllTransactions()
				.OrderBy(t => t.StockItemId)
				.ThenBy(t => t.TransactionDate)
				.ToList();

			int repairedCount = 0;

			var grouped = transactions.GroupBy(t => t.StockItemId);

			foreach (var itemGroup in grouped)
			{
				int balance = 0;

				foreach (var tx in itemGroup)
				{
					if (tx.Type == "IN")
						balance += tx.Quantity;
					else
						balance -= tx.Quantity;

					if (tx.BalanceAfter != balance)
					{
						tx.BalanceAfter = balance;
						_repository.UpdateTransactionBalance(tx.Id, balance);
						repairedCount++;
					}
				}
			}

			return repairedCount;
		}
	}
}
using System.Linq;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class LedgerIntegrityService
	{
		private readonly StockRepository _repository;

		public LedgerIntegrityService()
		{
			_repository = new StockRepository();
		}

		public (bool IsValid, int ErrorCount) ValidateLedger()
		{
			var transactions = _repository
				.GetAllTransactions()
				.OrderBy(t => t.StockItemId)
				.ThenBy(t => t.TransactionDate)
				.ThenBy(t => t.Id)
				.ToList();

			if (!transactions.Any())
				return (true, 0);

			int errors = 0;

			var groups = transactions.GroupBy(t => t.StockItemId);

			foreach (var group in groups)
			{
				int balance = 0;

				foreach (var tx in group)
				{
					if (tx.Type == "IN")
						balance += tx.Quantity;
					else
						balance -= tx.Quantity;

					if (tx.BalanceAfter != balance)
						errors++;
				}
			}

			return (errors == 0, errors);
		}
	}
}
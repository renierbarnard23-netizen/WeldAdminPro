using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialConsumptionService
	{
		private readonly StockTransactionRepository _transactionRepository;

		public MaterialConsumptionService()
		{
			_transactionRepository = new StockTransactionRepository();
		}

		public List<MaterialConsumptionStat> GetTopConsumed(int top = 10)
		{
			var transactions = _transactionRepository.GetAllTransactions();

			var stats = transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => new { t.ItemCode, t.ItemDescription })
				.Select(g => new MaterialConsumptionStat
				{
					ItemCode = g.Key.ItemCode ?? "",
					Description = g.Key.ItemDescription ?? "",
					TotalConsumed = g.Sum(t => t.Quantity),
					EstimatedCost = g.Sum(t => t.Quantity * t.UnitCost)
				})
				.OrderByDescending(s => s.TotalConsumed)
				.Take(top)
				.ToList();

			return stats;
		}
	}
}
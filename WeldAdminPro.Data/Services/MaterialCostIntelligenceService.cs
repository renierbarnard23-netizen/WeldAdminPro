using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialCostIntelligenceService
	{
		private readonly StockTransactionRepository _transactionRepository;

		public MaterialCostIntelligenceService()
		{
			_transactionRepository = new StockTransactionRepository();
		}

		public List<MaterialCostStat> GetTopCostDrivers(int top = 10)
		{
			var transactions = _transactionRepository.GetAllTransactions();

			var stats = transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => new { t.ItemCode, t.ItemDescription })
				.Select(g => new MaterialCostStat
				{
					ItemCode = g.Key.ItemCode ?? "",
					Description = g.Key.ItemDescription ?? "",
					TotalQuantity = g.Sum(t => t.Quantity),
					TotalCost = g.Sum(t => t.Quantity * t.UnitCost)
				})
				.OrderByDescending(s => s.TotalCost)
				.Take(top)
				.ToList();

			return stats;
		}
	}
}
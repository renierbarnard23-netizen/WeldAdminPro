using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialCostAnalysisService
	{
		private readonly StockRepository _repository;

		public MaterialCostAnalysisService()
		{
			_repository = new StockRepository();
		}

		public List<MaterialCostDriver> GetTopMaterialCostDrivers()
		{
			var transactions = _repository.GetAllTransactions();

			return transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => new { t.ItemCode, t.ItemDescription })
				.Select(g => new MaterialCostDriver
				{
					ItemCode = g.Key.ItemCode,
					Description = g.Key.ItemDescription,
					UnitsConsumed = g.Sum(x => x.Quantity),
					TotalCost = g.Sum(x => x.TransactionValue)
				})
				.OrderByDescending(x => x.TotalCost)
				.Take(20)
				.ToList();
		}
	}
}
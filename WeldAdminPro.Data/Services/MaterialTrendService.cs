using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialTrendService
	{
		private readonly StockRepository _repository;

		public MaterialTrendService()
		{
			_repository = new StockRepository();
		}

		public List<MaterialTrendModel> GetMaterialTrends()
		{
			var transactions = _repository.GetAllTransactions();

			var now = DateTime.Now;
			var thisMonthStart = new DateTime(now.Year, now.Month, 1);
			var lastMonthStart = thisMonthStart.AddMonths(-1);

			var thisMonth = transactions
				.Where(t => t.Type == "OUT"
						 && t.TransactionDate >= thisMonthStart);

			var lastMonth = transactions
				.Where(t => t.Type == "OUT"
						 && t.TransactionDate >= lastMonthStart
						 && t.TransactionDate < thisMonthStart);

			var materials = transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => new { t.ItemCode, t.ItemDescription });

			var results = new List<MaterialTrendModel>();

			foreach (var material in materials)
			{
				var lastMonthCost = lastMonth
					.Where(t => t.ItemCode == material.Key.ItemCode)
					.Sum(t => t.TransactionValue);

				var thisMonthCost = thisMonth
					.Where(t => t.ItemCode == material.Key.ItemCode)
					.Sum(t => t.TransactionValue);

				decimal percentChange = 0;

				if (lastMonthCost > 0)
				{
					percentChange =
						((thisMonthCost - lastMonthCost) / lastMonthCost) * 100;
				}

				results.Add(new MaterialTrendModel
				{
					ItemCode = material.Key.ItemCode,
					Description = material.Key.ItemDescription,
					LastMonthCost = lastMonthCost,
					ThisMonthCost = thisMonthCost,
					PercentChange = percentChange
				});
			}

			return results
				.OrderByDescending(x => Math.Abs(x.PercentChange))
				.ToList();
		}
	}
}
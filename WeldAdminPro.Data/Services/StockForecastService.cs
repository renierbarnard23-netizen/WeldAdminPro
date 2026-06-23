using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class StockForecastService
	{
		private readonly StockRepository _repository;

		public StockForecastService()
		{
			_repository = new StockRepository();
		}

		public List<StockForecastModel> GetStockForecast()
		{
			var transactions = _repository.GetAllTransactions();

			var now = DateTime.Now;
			var last90Days = now.AddDays(-90);

			var usage = transactions
				.Where(t => t.Type == "OUT" && t.TransactionDate >= last90Days)
				.GroupBy(t => new { t.ItemCode, t.ItemDescription });

			var results = new List<StockForecastModel>();

			foreach (var group in usage)
			{
				var itemCode = group.Key.ItemCode;
				var description = group.Key.ItemDescription;

				var totalUsed = group.Sum(x => x.Quantity);

				var avgMonthlyUse = totalUsed / 3;

				var currentStock = transactions
					.Where(t => t.ItemCode == itemCode)
					.Sum(t => t.Type == "IN" ? t.Quantity : -t.Quantity);

				decimal daysRemaining;

				if (avgMonthlyUse <= 0)
				{
					// No usage history
					daysRemaining = 9999;
				}
				else
				{
					var dailyUse = avgMonthlyUse / 30m;

					if (dailyUse <= 0)
					{
						daysRemaining = 9999;
					}
					else
					{
						daysRemaining = currentStock / dailyUse;
					}
				}

				var status = CalculateStatus(daysRemaining);

				results.Add(new StockForecastModel
				{
					ItemCode = itemCode,
					Description = description,
					CurrentStock = currentStock,
					AvgMonthlyUse = avgMonthlyUse,
					DaysRemaining = daysRemaining,
					Status = status
				});
			}

			return results
				.OrderBy(x => x.DaysRemaining)
				.ToList();
		}

		private string CalculateStatus(decimal daysRemaining)
		{
			if (daysRemaining <= 7)
				return "Critical";

			if (daysRemaining <= 30)
				return "Low";

			if (daysRemaining <= 60)
				return "Watch";

			return "Safe";
		}
	}
}
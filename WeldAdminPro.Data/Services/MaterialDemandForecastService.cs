using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialDemandForecastService
	{
		private readonly StockRepository _stockRepository;
		private readonly StockTransactionRepository _transactionRepository;

		public MaterialDemandForecastService()
		{
			_stockRepository = new StockRepository();
			_transactionRepository = new StockTransactionRepository();
		}

		public List<MaterialDemandForecast> GenerateForecast(int daysHistory = 30)
		{
			var items = _stockRepository.GetAll();
			var transactions = _transactionRepository.GetAllTransactions();

			var cutoff = DateTime.Now.AddDays(-daysHistory);

			var forecasts = new List<MaterialDemandForecast>();

			foreach (var item in items)
			{
				var usage = transactions
					.Where(t => t.StockItemId == item.Id)
					.Where(t => t.Type == "OUT")
					.Where(t => t.TransactionDate >= cutoff)
					.Sum(t => t.Quantity);

				double avgDaily = usage / (double)daysHistory;

				int daysRemaining = avgDaily > 0
					? (int)(item.Quantity / avgDaily)
					: int.MaxValue;

				string status = "OK";

				if (daysRemaining < 7)
					status = "Critical";

				else if (daysRemaining < 30)
					status = "Reorder Soon";

				forecasts.Add(new MaterialDemandForecast
				{
					ItemCode = item.ItemCode,
					Description = item.Description,
					CurrentStock = item.Quantity,
					AverageDailyConsumption = avgDaily,
					DaysRemaining = daysRemaining,
					Status = status
				});
			}

			return forecasts
				.OrderBy(f => f.DaysRemaining)
				.ToList();
		}
	}
}
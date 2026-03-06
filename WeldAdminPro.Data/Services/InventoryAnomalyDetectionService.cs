using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class InventoryAnomalyDetectionService
	{
		private readonly StockTransactionRepository _transactionRepository;

		public InventoryAnomalyDetectionService()
		{
			_transactionRepository = new StockTransactionRepository();
		}

		public List<InventoryAnomaly> DetectAnomalies(int recentDays = 7, int historyDays = 30)
		{
			var transactions = _transactionRepository.GetAllTransactions();

			var recentCutoff = DateTime.Now.AddDays(-recentDays);
			var historyCutoff = DateTime.Now.AddDays(-historyDays);

			var anomalies = new List<InventoryAnomaly>();

			var grouped = transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription });

			foreach (var group in grouped)
			{
				var recentUsage = group
					.Where(t => t.TransactionDate >= recentCutoff)
					.Sum(t => t.Quantity);

				var historicalUsage = group
					.Where(t => t.TransactionDate >= historyCutoff && t.TransactionDate < recentCutoff)
					.Sum(t => t.Quantity);

				double historicalAvg = historicalUsage / Math.Max(1, historyDays - recentDays);

				double recentAvg = recentUsage / Math.Max(1, recentDays);

				if (historicalAvg == 0)
					continue;

				double increase = ((recentAvg - historicalAvg) / historicalAvg) * 100;

				if (increase > 100)
				{
					string severity = increase > 300 ? "Critical"
									: increase > 200 ? "High"
									: "Medium";

					anomalies.Add(new InventoryAnomaly
					{
						ItemCode = group.Key.ItemCode ?? "",
						Description = group.Key.ItemDescription ?? "",
						CurrentConsumption = recentAvg,
						HistoricalAverage = historicalAvg,
						IncreasePercentage = increase,
						Severity = severity
					});
				}
			}

			return anomalies
				.OrderByDescending(a => a.IncreasePercentage)
				.ToList();
		}
	}
}
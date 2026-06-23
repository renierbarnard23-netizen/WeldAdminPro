using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Core.Services
{
	public class SmartStockControlService
	{
		private readonly StockRepository _repository;

		public SmartStockControlService()
		{
			_repository = new StockRepository();
		}

		public SmartStockInsight Analyze(StockItem item, int lookbackDays = 60)
		{
			var transactions = _repository
				.GetAllTransactions()
				.Where(t =>
					t.StockItemId == item.Id &&
					t.Type == "OUT" &&
					t.TransactionDate >= DateTime.UtcNow.AddDays(-lookbackDays))
				.ToList();

			var totalIssued = transactions.Sum(t => t.Quantity);

			decimal avgDailyUsage = 0;

			if (lookbackDays > 0)
				avgDailyUsage = (decimal)totalIssued / lookbackDays;

			int daysRemaining = 0;

			if (avgDailyUsage > 0)
				daysRemaining = (int)Math.Ceiling((decimal)item.Quantity / avgDailyUsage);

			int reorderPoint = (int)Math.Ceiling(avgDailyUsage * 14); // 2-week buffer
			int reorderQty = (int)Math.Ceiling(avgDailyUsage * 30);   // 1 month stock

			var risk = SmartStockRiskLevel.Normal;

			if (daysRemaining <= 7)
				risk = SmartStockRiskLevel.Critical;
			else if (daysRemaining <= 14)
				risk = SmartStockRiskLevel.Warning;

			if (item.Quantity <= 0)
				risk = SmartStockRiskLevel.Critical;

			return new SmartStockInsight
			{
				StockItemId = item.Id,
				AverageDailyUsage = avgDailyUsage,
				DaysRemaining = daysRemaining,
				SuggestedReorderPoint = reorderPoint,
				SuggestedReorderQuantity = reorderQty,
				RiskLevel = risk
			};
		}

		public Dictionary<Guid, SmartStockInsight> AnalyzeAll(IEnumerable<StockItem> items)
		{
			return items.ToDictionary(
				item => item.Id,
				item => Analyze(item)
			);
		}
	}
}

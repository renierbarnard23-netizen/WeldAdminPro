using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
	public class StockAnalyticsResult
	{
		public List<ItemMovementSummary> ItemSummaries { get; set; } = new();
		public List<MonthlyMovementSummaryDto> MonthlySummaries { get; set; } = new();

		public string TopMovingItem { get; set; } = "-";
		public decimal TopMovingItemValue { get; set; }

		public string MostConsumedItem { get; set; } = "-";
		public int MostConsumedUnits { get; set; }

		public string HighestGrowthItem { get; set; } = "-";
		public int HighestGrowthUnits { get; set; }

		public int DeadStockCount { get; set; }
	}

	public class MonthlyMovementSummaryDto
	{
		public DateTime Month { get; set; }
		public int TotalIn { get; set; }
		public int TotalOut { get; set; }
		public decimal ValueIn { get; set; }
		public decimal ValueOut { get; set; }
	}

	public class StockAnalyticsService
	{
		public StockAnalyticsResult BuildAnalytics(
			List<StockTransaction> transactions,
			List<StockItem> allItems)
		{
			var result = new StockAnalyticsResult();

			if (!transactions.Any())
				return result;

			const decimal periodDays = 30m;

			result.ItemSummaries = transactions
				.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
				.Select(g =>
				{
					var item = allItems.FirstOrDefault(i => i.Id == g.Key.StockItemId);

					var totalIn = g.Sum(x => x.QtyIn);
					var totalOut = g.Sum(x => x.QtyOut);
					var closingQty = item?.Quantity ?? 0;

					decimal avgInventory = closingQty > 0 ? closingQty : 1;
					decimal turnover = totalOut / avgInventory;
					decimal daysInInventory = turnover > 0 ? periodDays / turnover : 0;

					string category;

					if (totalIn == 0 && totalOut == 0)
						category = "Dead Stock";
					else if (totalOut > 0 && totalOut >= totalIn)
						category = "High Consumption";
					else if (totalOut > 0)
						category = "Fast Moving";
					else if (totalIn > 0 && totalOut == 0)
						category = "New Stock";
					else
						category = "Slow Moving";

					return new ItemMovementSummary
					{
						StockItemId = g.Key.StockItemId,
						ItemCode = g.Key.ItemCode,
						Description = g.Key.ItemDescription,
						TotalIn = totalIn,
						TotalOut = totalOut,
						MovementValue = g.Sum(x => x.TransactionValue),
						CurrentBalance = closingQty,
						CurrentStockValue = item != null
							? item.Quantity * item.AverageUnitCost
							: 0,
						AverageInventory = Math.Round(avgInventory, 2),
						TurnoverRate = Math.Round(turnover, 2),
						DaysInInventory = Math.Round(daysInInventory, 1),
						MovementCategory = category
					};
				})
				.OrderByDescending(x => x.MovementValue)
				.ToList();

			var topValueItem = result.ItemSummaries.FirstOrDefault();
			if (topValueItem != null)
			{
				result.TopMovingItem = topValueItem.ItemCode;
				result.TopMovingItemValue = topValueItem.MovementValue;
			}

			var mostConsumed = result.ItemSummaries
				.OrderByDescending(x => x.TotalOut)
				.FirstOrDefault();

			if (mostConsumed != null)
			{
				result.MostConsumedItem = mostConsumed.ItemCode;
				result.MostConsumedUnits = mostConsumed.TotalOut;
			}

			var highestGrowth = result.ItemSummaries
				.OrderByDescending(x => x.NetMovement)
				.FirstOrDefault();

			if (highestGrowth != null)
			{
				result.HighestGrowthItem = highestGrowth.ItemCode;
				result.HighestGrowthUnits = highestGrowth.NetMovement;
			}

			result.DeadStockCount =
				result.ItemSummaries.Count(x => x.TotalIn == 0 && x.TotalOut == 0);

			result.MonthlySummaries = transactions
				.GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1))
				.OrderBy(g => g.Key)
				.Select(g => new MonthlyMovementSummaryDto
				{
					Month = g.Key,
					TotalIn = g.Sum(x => x.QtyIn),
					TotalOut = g.Sum(x => x.QtyOut),
					ValueIn = g.Where(x => x.Type == "IN").Sum(x => x.TransactionValue),
					ValueOut = g.Where(x => x.Type == "OUT").Sum(x => x.TransactionValue)
				})
				.ToList();

			return result;
		}
	}
}
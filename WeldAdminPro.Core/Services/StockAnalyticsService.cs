using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Enums;

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

		public int NetMovement => TotalIn - TotalOut;
		public decimal NetValue => ValueIn - ValueOut;
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

			var globalFirstDate = transactions.Min(t => t.TransactionDate).Date;
			var globalLastDate = transactions.Max(t => t.TransactionDate).Date;
			var globalPeriodDays = Math.Max((globalLastDate - globalFirstDate).TotalDays, 1);

			result.ItemSummaries = transactions
				.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
				.Select(g =>
				{
					var item = allItems.FirstOrDefault(i => i.Id == g.Key.StockItemId);

					int totalIn = g.Sum(x => x.QtyIn);
					int totalOut = g.Sum(x => x.QtyOut);

					decimal closingQtyDecimal = item?.Quantity ?? 0m;
					int closingQty = (int)Math.Floor(closingQtyDecimal);

					decimal avgInventory = closingQtyDecimal > 0 ? closingQtyDecimal : 1m;
					decimal turnover = totalOut / avgInventory;
					decimal daysInInventory = turnover > 0 ? 30m / turnover : 0m;

					// ================= INVENTORY CATEGORY =================

					ItemInventoryCategory category;

					if (totalIn == 0 && totalOut == 0 && closingQty == 0)
						category = ItemInventoryCategory.Inactive;
					else if (totalIn == 0 && totalOut == 0 && closingQty > 0)
						category = ItemInventoryCategory.Stocked;
					else if (totalIn > 0 && totalOut == 0)
						category = ItemInventoryCategory.Replenished;
					else if (totalOut > 0 && totalIn == 0)
						category = ItemInventoryCategory.ActiveConsumption;
					else
						category = ItemInventoryCategory.Balanced;

					// ================= HEALTH STATUS =================

					InventoryHealthStatus health;

					if (category == ItemInventoryCategory.Inactive)
						health = InventoryHealthStatus.DeadStock;
					else if (daysInInventory > 120)
						health = InventoryHealthStatus.AgingRisk;
					else if (turnover < 0.2m && totalOut > 0)
						health = InventoryHealthStatus.SlowMoving;
					else if (closingQty < 5 && totalOut > 0)
						health = InventoryHealthStatus.StockoutRisk;
					else
						health = InventoryHealthStatus.Healthy;

					// ================= USAGE =================

					decimal averageDailyUsage = totalOut / (decimal)globalPeriodDays;

					decimal daysUntilStockout =
						averageDailyUsage > 0
							? closingQtyDecimal / averageDailyUsage
							: 0m;

					DateTime? estimatedStockoutDate =
						averageDailyUsage > 0
							? DateTime.Today.AddDays((double)daysUntilStockout)
							: null;

					// ================= INDUSTRIAL MIN/MAX REORDER =================

					decimal minLevel = item?.MinLevel ?? 0m;
decimal maxLevel = item?.MaxLevel ?? 0m;

					int suggestedReorderQty = 0;

					if (maxLevel > 0)
					{
						if (closingQtyDecimal <= minLevel)
						{
							suggestedReorderQty = (int)Math.Ceiling(maxLevel - closingQtyDecimal);
							if (suggestedReorderQty < 0)
								suggestedReorderQty = 0;
						}
					}
					else
					{
						const int targetCoverageDays = 30;
						decimal targetStock = averageDailyUsage * targetCoverageDays;

						if (averageDailyUsage > 0 && closingQtyDecimal < targetStock)
							suggestedReorderQty =
								(int)Math.Ceiling(targetStock - closingQtyDecimal);

						if (suggestedReorderQty > totalOut * 2)
							suggestedReorderQty = totalOut * 2;
					}

					decimal suggestedOrderValue =
						item != null
							? suggestedReorderQty * item.AverageUnitCost
							: 0m;

					// ================= RISK =================

					string reorderRisk;

					if (averageDailyUsage == 0)
						reorderRisk = "No Usage";
					else if (closingQty == 0)
						reorderRisk = "Critical";
					else if (closingQty <= minLevel)
						reorderRisk = "ReorderRequired";
					else if (daysUntilStockout < 7)
						reorderRisk = "High";
					else if (daysUntilStockout < 30)
						reorderRisk = "Medium";
					else
						reorderRisk = "Low";

					return new ItemMovementSummary
					{
						StockItemId = g.Key.StockItemId,
						ItemCode = g.Key.ItemCode,
						Description = g.Key.ItemDescription,
						TotalIn = totalIn,
						TotalOut = totalOut,
						MovementValue = g.Sum(x => x.TransactionValue),
						CurrentBalance = closingQtyDecimal,
						CurrentStockValue = item != null
							? item.Quantity * item.AverageUnitCost
							: 0m,
						AverageInventory = Math.Round(avgInventory, 2),
						TurnoverRate = Math.Round(turnover, 2),
						DaysInInventory = Math.Round(daysInInventory, 1),
						InventoryCategory = category,
						HealthStatus = health,
						AverageDailyUsage = Math.Round(averageDailyUsage, 2),
						DaysUntilStockout = Math.Round(daysUntilStockout, 1),
						EstimatedStockoutDate = estimatedStockoutDate,
						SuggestedReorderQuantity = suggestedReorderQty,
						SuggestedOrderValue = suggestedOrderValue,
						ReorderRiskLevel = reorderRisk
					};
				})
				.OrderByDescending(x => x.MovementValue)
				.ToList();

			// ================= KPI =================

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
				result.ItemSummaries.Count(x =>
					x.InventoryCategory == ItemInventoryCategory.Inactive);

			// ================= MONTHLY =================

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
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

		public decimal TotalInventoryValue { get; set; }
		public decimal CapitalLockedValue { get; set; }

		public decimal CapitalLockedPercentage =>
			TotalInventoryValue > 0
				? Math.Round(CapitalLockedValue / TotalInventoryValue * 100m, 2)
				: 0m;

		public int AItemCount { get; set; }
		public int BItemCount { get; set; }
		public int CItemCount { get; set; }

		public decimal AValue { get; set; }
		public decimal BValue { get; set; }
		public decimal CValue { get; set; }

		public int CriticalACount { get; set; }
		public int HighRiskCount { get; set; }
		public int MediumRiskCount { get; set; }
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

			result.ItemSummaries = BuildItemSummaries(transactions, allItems);

			ApplyABCPareto(result.ItemSummaries);
			ApplyRiskHeatmap(result.ItemSummaries);
			ApplyReorderPolicy(result.ItemSummaries);

			// NEW CLASSIFICATION ENGINE
			ApplyMovementClassification(result.ItemSummaries);

			// NEW ENGINE
			ApplyUsageVarianceDetection(result.ItemSummaries, transactions);

			BuildKPIs(result);

			result.MonthlySummaries = BuildMonthlySummaries(transactions);

			CalculateCapitalExposure(result);

			return result;
		}

		// =========================================================
		// CAPITAL EXPOSURE CALCULATION
		// =========================================================

		private void CalculateCapitalExposure(StockAnalyticsResult result)
		{
			result.TotalInventoryValue =
				result.ItemSummaries.Sum(x => x.CurrentStockValue);

			result.CapitalLockedValue =
				result.ItemSummaries
					.Where(x => x.ReorderRiskLevel == "Capital Lock Risk")
					.Sum(x => x.CurrentStockValue);

			result.AItemCount =
				result.ItemSummaries.Count(x => x.ABCClass == "A");

			result.BItemCount =
				result.ItemSummaries.Count(x => x.ABCClass == "B");

			result.CItemCount =
				result.ItemSummaries.Count(x => x.ABCClass == "C");

			result.AValue =
				result.ItemSummaries
					.Where(x => x.ABCClass == "A")
					.Sum(x => x.CurrentStockValue);

			result.BValue =
				result.ItemSummaries
					.Where(x => x.ABCClass == "B")
					.Sum(x => x.CurrentStockValue);

			result.CValue =
				result.ItemSummaries
					.Where(x => x.ABCClass == "C")
					.Sum(x => x.CurrentStockValue);

			result.CriticalACount =
				result.ItemSummaries.Count(x => x.ReorderRiskLevel == "Critical-A");

			result.HighRiskCount =
				result.ItemSummaries.Count(x => x.ReorderRiskLevel == "High");

			result.MediumRiskCount =
				result.ItemSummaries.Count(x => x.ReorderRiskLevel == "Medium");
		}

		// =========================================================
		// BUILD BASE METRICS
		// =========================================================

		private List<ItemMovementSummary> BuildItemSummaries(
			List<StockTransaction> transactions,
			List<StockItem> allItems)
		{
			var globalFirstDate = transactions.Min(t => t.TransactionDate).Date;
			var globalLastDate = transactions.Max(t => t.TransactionDate).Date;
			var globalPeriodDays =
				Math.Max((globalLastDate - globalFirstDate).TotalDays, 1);

			return transactions
				.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
				.Select(g =>
				{
					var item = allItems.FirstOrDefault(i => i.Id == g.Key.StockItemId);

					int totalIn = g.Sum(x => x.QtyIn);
					int totalOut = g.Sum(x => x.QtyOut);

					decimal closingQty = item?.Quantity ?? 0m;
					decimal avgInventory = closingQty > 0 ? closingQty : 1m;

					decimal turnover = totalOut / avgInventory;
					decimal daysInInventory =
						turnover > 0 ? 30m / turnover : 0m;

					decimal averageDailyUsage =
						totalOut / (decimal)globalPeriodDays;

					decimal daysUntilStockout =
						averageDailyUsage > 0
							? closingQty / averageDailyUsage
							: 0m;

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
							: 0m,
						AverageInventory = Math.Round(avgInventory, 2),
						TurnoverRate = Math.Round(turnover, 2),
						DaysInInventory = Math.Round(daysInInventory, 1),
						AverageDailyUsage = Math.Round(averageDailyUsage, 2),
						DaysUntilStockout = Math.Round(daysUntilStockout, 1),
						EstimatedStockoutDate =
							averageDailyUsage > 0
								? DateTime.Today.AddDays((double)daysUntilStockout)
								: null,
						SuggestedReorderQuantity = 0,
						SuggestedOrderValue = 0,
						ReorderRiskLevel = "Low"
					};
				})
				.OrderByDescending(x => x.MovementValue)
				.ToList();
		}

		// =========================================================
		// ABC PARETO (CORRECTED)
		// =========================================================

		private void ApplyABCPareto(List<ItemMovementSummary> items)
		{
			var ordered = items
				.OrderByDescending(x => x.MovementValue)
				.ToList();

			decimal total = ordered.Sum(x => x.MovementValue);
			decimal cumulative = 0m;

			decimal maxValue = ordered.Any()
				? ordered.Max(x => x.MovementValue)
				: 1m;

			if (maxValue <= 0)
				maxValue = 1m;

			const double maxBarWidth = 600; // pixels

			foreach (var item in ordered)
			{
				cumulative += item.MovementValue;

				decimal cumulativePercentage =
					total > 0 ? cumulative / total * 100m : 0m;

				item.CumulativePercentage =
					Math.Round(cumulativePercentage, 2);

				// ABC classification
				if (cumulativePercentage <= 80m)
					item.ABCClass = "A";
				else if (cumulativePercentage <= 95m)
					item.ABCClass = "B";
				else
					item.ABCClass = "C";

				// NORMALIZED BAR WIDTH
				item.ParetoBarWidth =
					(double)(item.MovementValue / maxValue * (decimal)maxBarWidth);
			}
		}
		private void ApplyRiskHeatmap(List<ItemMovementSummary> items)
		{
			if (!items.Any())
				return;

			var orderedByValue = items
				.OrderByDescending(x => x.MovementValue)
				.ToList();

			int total = orderedByValue.Count;

			for (int i = 0; i < total; i++)
			{
				var item = orderedByValue[i];

				// VALUE TIER
				string valueTier =
					i < total * 0.3 ? "High" :
					i < total * 0.7 ? "Medium" :
					"Low";

				// USAGE TIER (based on TurnoverRate)
				string usageTier =
					item.TurnoverRate > 10 ? "High" :
					item.TurnoverRate > 3 ? "Medium" :
					"Low";

				item.RiskHeatZone = $"{valueTier}-{usageTier}";
			}
		}

		// =========================================================
		// REORDER POLICY
		// =========================================================

		private void ApplyReorderPolicy(List<ItemMovementSummary> items)
		{
			decimal totalPortfolioValue =
				items.Sum(x => x.CurrentStockValue);

			if (totalPortfolioValue <= 0)
				totalPortfolioValue = 1m;

			decimal capitalLockThreshold =
				totalPortfolioValue * 0.05m;

			foreach (var item in items)
			{
				if (item.ABCClass == "C"
					&& item.AverageDailyUsage < 0.1m
					&& item.CurrentStockValue > capitalLockThreshold)
				{
					item.ReorderRiskLevel = "Capital Lock Risk";
					continue;
				}

				if (item.AverageDailyUsage <= 0)
				{
					item.ReorderRiskLevel = "No Usage";
					continue;
				}

				if (item.ABCClass == "A" && item.DaysUntilStockout < 14)
					item.ReorderRiskLevel = "Critical-A";
				else if (item.DaysUntilStockout < 7)
					item.ReorderRiskLevel = "High";
				else if (item.DaysUntilStockout < 30)
					item.ReorderRiskLevel = "Medium";
				else
					item.ReorderRiskLevel = "Low";
			}
		}

		// =========================================================
		// MOVEMENT CLASSIFICATION ENGINE
		// =========================================================

		private void ApplyMovementClassification(List<ItemMovementSummary> items)
		{
			foreach (var item in items)
			{
				// DEAD STOCK
				if (item.TotalOut == 0 && item.CurrentBalance > 0)
				{
					item.InventoryCategory = ItemInventoryCategory.Inactive;
					continue;
				}

				// FAST MOVING ITEMS
				if (item.TurnoverRate >= 12 || item.AverageDailyUsage >= 5)
				{
					item.InventoryCategory = ItemInventoryCategory.FastMoving;
					continue;
				}

				// MODERATE MOVEMENT
				if (item.TurnoverRate >= 4)
				{
					item.InventoryCategory = ItemInventoryCategory.Moderate;
					continue;
				}

				// SLOW MOVING
				if (item.TurnoverRate > 0)
				{
					item.InventoryCategory = ItemInventoryCategory.SlowMoving;
					continue;
				}

				// NO MOVEMENT
				item.InventoryCategory = ItemInventoryCategory.Inactive;
			}
		}

		// =========================================================
		// KPI BUILDER
		// =========================================================

		// =========================================================
		// USAGE VARIANCE DETECTION
		// =========================================================

		private void ApplyUsageVarianceDetection(
			List<ItemMovementSummary> items,
			List<StockTransaction> transactions)
		{
			foreach (var item in items)
			{
				var monthlyUsage = transactions
					.Where(t => t.StockItemId == item.StockItemId)
					.GroupBy(t => new DateTime(
						t.TransactionDate.Year,
						t.TransactionDate.Month,
						1))
					.Select(g => g.Sum(x => x.QtyOut))
					.ToList();

				if (monthlyUsage.Count < 2)
				{
					item.UsageVarianceScore = 0;
					item.DemandPattern = "Insufficient Data";
					continue;
				}

				decimal avg = (decimal)monthlyUsage.Average();

				if (avg == 0)
				{
					item.UsageVarianceScore = 0;
					item.DemandPattern = "No Demand";
					continue;
				}

				decimal variance =
					monthlyUsage.Sum(v => (decimal)Math.Pow((double)(v - avg), 2))
					/ monthlyUsage.Count;

				decimal stdDev = Convert.ToDecimal(Math.Sqrt((double)variance));

				decimal coefficientOfVariation = stdDev / avg;

				item.UsageVarianceScore =
					Math.Round(coefficientOfVariation, 2);

				if (coefficientOfVariation < 0.25m)
					item.DemandPattern = "Stable";
				else if (coefficientOfVariation < 0.75m)
					item.DemandPattern = "Fluctuating";
				else
					item.DemandPattern = "Volatile";
			}
		}
		private void BuildKPIs(StockAnalyticsResult result)
		{
			var topValue = result.ItemSummaries
				.OrderByDescending(x => x.MovementValue)
				.FirstOrDefault();

			if (topValue != null)
			{
				result.TopMovingItem = topValue.ItemCode;
				result.TopMovingItemValue = topValue.MovementValue;
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
		}

		private List<MonthlyMovementSummaryDto> BuildMonthlySummaries(
			List<StockTransaction> transactions)
		{
			return transactions
				.GroupBy(t => new DateTime(
					t.TransactionDate.Year,
					t.TransactionDate.Month,
					1))
				.OrderBy(g => g.Key)
				.Select(g => new MonthlyMovementSummaryDto
				{
					Month = g.Key,
					TotalIn = g.Sum(x => x.QtyIn),
					TotalOut = g.Sum(x => x.QtyOut),
					ValueIn = g.Where(x => x.Type == "IN")
							   .Sum(x => x.TransactionValue),
					ValueOut = g.Where(x => x.Type == "OUT")
								.Sum(x => x.TransactionValue)
				})
				.ToList();
		}
	}
}
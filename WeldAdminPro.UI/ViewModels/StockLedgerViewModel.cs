using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Reporting;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using System.IO;

namespace WeldAdminPro.UI.ViewModels
{
	public class MonthlyMovementSummary
	{
		public DateTime Month { get; set; }
		public int TotalIn { get; set; }
		public int TotalOut { get; set; }
		public int NetMovement => TotalIn - TotalOut;
		public decimal ValueIn { get; set; }
		public decimal ValueOut { get; set; }
		public decimal NetValue => ValueIn - ValueOut;
	}

	public class StockLedgerGroup
	{
		public string ItemCode { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public ObservableCollection<StockTransaction> Transactions { get; set; }
			= new ObservableCollection<StockTransaction>();

		public int CurrentBalance =>
			Transactions.LastOrDefault()?.BalanceAfter ?? 0;

		public int TotalIn => Transactions.Sum(t => t.QtyIn);
		public int TotalOut => Transactions.Sum(t => t.QtyOut);
		public int NetMovement => TotalIn - TotalOut;

		public decimal TotalMovementValue =>
			Transactions.Sum(t => t.TransactionValue);
	}

	public partial class StockLedgerViewModel : ObservableObject
	{
		private readonly StockRepository _repository;
		private readonly StockAnalyticsService _analyticsService;

		public IRelayCommand RecalculateCommand { get; }
		public IRelayCommand ApplyFilterCommand { get; }

		public IRelayCommand ExportExecutiveReportCommand { get; }

		// =============================
		// GENERAL DASHBOARD METRICS
		// =============================

		[ObservableProperty] private int totalStockItems;
		[ObservableProperty] private int totalUnitsInStock;
		[ObservableProperty] private decimal dashboardInventoryValue;
		[ObservableProperty] private int totalTransactions;

		[ObservableProperty] private DateTime? startDate;
		[ObservableProperty] private DateTime? endDate;

		[ObservableProperty] private int periodTotalIn;
		[ObservableProperty] private int periodTotalOut;
		[ObservableProperty] private int periodNetMovement;
		[ObservableProperty] private decimal periodValueIn;
		[ObservableProperty] private decimal periodValueOut;
		[ObservableProperty] private decimal periodNetValue;

		// =============================
		// EXECUTIVE KPIs
		// =============================

		[ObservableProperty] private string topMovingItem = "-";
		[ObservableProperty] private decimal topMovingItemValue;
		[ObservableProperty] private string mostConsumedItem = "-";
		[ObservableProperty] private int mostConsumedUnits;
		[ObservableProperty] private string highestGrowthItem = "-";
		[ObservableProperty] private int highestGrowthUnits;
		[ObservableProperty] private int deadStockCount;

		[ObservableProperty] private decimal totalInventoryValue;
		[ObservableProperty] private decimal capitalLockedValue;
		[ObservableProperty] private decimal capitalLockedPercentage;

		[ObservableProperty] private int aItemCount;
		[ObservableProperty] private int bItemCount;
		[ObservableProperty] private int cItemCount;

		[ObservableProperty] private int criticalACount;
		[ObservableProperty] private int highRiskCount;
		[ObservableProperty] private int mediumRiskCount;

		[ObservableProperty] private string executiveRiskLevel = "Healthy";
		[ObservableProperty] private string executiveRiskColor = "#2E7D32"; // Green default

		[ObservableProperty] private decimal aPercentage;
		[ObservableProperty] private decimal bPercentage;
		[ObservableProperty] private decimal cPercentage;

		[ObservableProperty]
		private ObservableCollection<ItemMovementSummary> paretoItems = new();

		[ObservableProperty] private int inventoryHealthScore;
		[ObservableProperty] private string inventoryHealthLabel = "Healthy";
		[ObservableProperty] private string inventoryHealthColor = "#2E7D32";

		private void CalculateHealthScore()
{
	int score =
		100
		- (CriticalACount * 20)
		- (HighRiskCount * 10)
		- (MediumRiskCount * 5)
		- (int)(CapitalLockedPercentage / 2);

	if (score < 0) score = 0;
	if (score > 100) score = 100;

	InventoryHealthScore = score;

	if (score >= 85)
	{
		InventoryHealthLabel = "STRONG";
		InventoryHealthColor = "#2E7D32"; // Green
	}
	else if (score >= 60)
	{
		InventoryHealthLabel = "WATCH";
		InventoryHealthColor = "#F9A825"; // Yellow
	}
	else
	{
		InventoryHealthLabel = "RISK";
		InventoryHealthColor = "#C62828"; // Red
	}
}

		// =============================
		// DATA COLLECTIONS
		// =============================

		[ObservableProperty] private ObservableCollection<StockLedgerGroup> ledgerGroups = new();
		[ObservableProperty] private ObservableCollection<MonthlyMovementSummary> monthlySummaries = new();
		[ObservableProperty] private ObservableCollection<ItemMovementSummary> itemMovementSummaries = new();
		[ObservableProperty] private ObservableCollection<ItemMovementSummary> reorderAlerts = new();
		public ObservableCollection<RiskMatrixCell> RiskMatrix { get; set; }
	= new ObservableCollection<RiskMatrixCell>();

		public bool HasLedgerMismatch =>
			LedgerGroups.Any(g =>
				g.Transactions.Any(t => t.IsBalanceMismatch));

		public StockLedgerViewModel()
		{
			_repository = new StockRepository();
			_analyticsService = new StockAnalyticsService();

			RecalculateCommand = new RelayCommand(RecalculateBalances);
			ApplyFilterCommand = new RelayCommand(LoadLedger);
			ExportExecutiveReportCommand = new RelayCommand(ExportExecutiveReport);

			LoadLedger();
		}

		private void RecalculateBalances()
		{
			_repository.RecalculateAllBalances();
			LoadLedger();
		}

		private void LoadLedger()
		{
			var allItems = _repository.GetAll();
			var allTransactions = _repository.GetAllTransactions();

			TotalStockItems = allItems.Count;
			TotalUnitsInStock = allItems.Sum(i => i.Quantity);
			DashboardInventoryValue = allItems.Sum(i => i.Quantity * i.AverageUnitCost);
			TotalTransactions = allTransactions.Count;

			IEnumerable<StockTransaction> filtered = allTransactions;

			if (StartDate.HasValue)
				filtered = filtered.Where(t => t.TransactionDate.Date >= StartDate.Value.Date);

			if (EndDate.HasValue)
				filtered = filtered.Where(t => t.TransactionDate.Date <= EndDate.Value.Date);

			var transactions = filtered
				.OrderBy(t => t.TransactionDate)
				.ThenBy(t => t.Id)
				.ToList();

			if (!transactions.Any())
			{
				ItemMovementSummaries = new();
				MonthlySummaries = new();
				LedgerGroups = new();
				ReorderAlerts = new();
				return;
			}

			PeriodTotalIn = transactions.Sum(t => t.QtyIn);
			PeriodTotalOut = transactions.Sum(t => t.QtyOut);
			PeriodNetMovement = PeriodTotalIn - PeriodTotalOut;

			PeriodValueIn = transactions.Where(t => t.Type == "IN").Sum(t => t.TransactionValue);
			PeriodValueOut = transactions.Where(t => t.Type == "OUT").Sum(t => t.TransactionValue);
			PeriodNetValue = PeriodValueIn - PeriodValueOut;

			var analytics = _analyticsService.BuildAnalytics(transactions, allItems);
			
			ItemMovementSummaries = new(analytics.ItemSummaries);
			BuildRiskMatrix(analytics.ItemSummaries);
			ParetoItems = new(
	analytics.ItemSummaries
		.OrderByDescending(x => x.MovementValue)
		.Take(15) // top 15 for clarity
);

			// Executive KPIs
			TotalInventoryValue = analytics.TotalInventoryValue;
			if (TotalInventoryValue > 0)
			{
				APercentage = Math.Round((analytics.AValue / TotalInventoryValue) * 100m, 1);
				BPercentage = Math.Round((analytics.BValue / TotalInventoryValue) * 100m, 1);
				CPercentage = Math.Round((analytics.CValue / TotalInventoryValue) * 100m, 1);
			}
			else
			{
				APercentage = 0;
				BPercentage = 0;
				CPercentage = 0;
			}
			CapitalLockedValue = analytics.CapitalLockedValue;
			CapitalLockedPercentage = analytics.CapitalLockedPercentage;

			AItemCount = analytics.AItemCount;
			BItemCount = analytics.BItemCount;
			CItemCount = analytics.CItemCount;

			CriticalACount = analytics.CriticalACount;
			HighRiskCount = analytics.HighRiskCount;
			MediumRiskCount = analytics.MediumRiskCount;
			EvaluateExecutiveRisk();
			CalculateHealthScore();

			TopMovingItem = analytics.TopMovingItem;
			TopMovingItemValue = analytics.TopMovingItemValue;
			MostConsumedItem = analytics.MostConsumedItem;
			MostConsumedUnits = analytics.MostConsumedUnits;
			HighestGrowthItem = analytics.HighestGrowthItem;
			HighestGrowthUnits = analytics.HighestGrowthUnits;
			DeadStockCount = analytics.DeadStockCount;

			ItemMovementSummaries = new(analytics.ItemSummaries);

			ReorderAlerts = new(
				analytics.ItemSummaries
					.Where(x =>
						x.ReorderRiskLevel == "Critical-A" ||
						x.ReorderRiskLevel == "High" ||
						x.ReorderRiskLevel == "Medium")
					.OrderBy(x => x.DaysUntilStockout)
			);

			MonthlySummaries = new(
				analytics.MonthlySummaries.Select(m => new MonthlyMovementSummary
				{
					Month = m.Month,
					TotalIn = m.TotalIn,
					TotalOut = m.TotalOut,
					ValueIn = m.ValueIn,
					ValueOut = m.ValueOut
				}));

			LedgerGroups = new(
				transactions
					.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
					.OrderBy(g => g.Key.ItemCode)
					.Select(g =>
					{
						var ordered = g.ToList();
						ValidateRunningBalance(ordered);

						return new StockLedgerGroup
						{
							ItemCode = g.Key.ItemCode,
							Description = g.Key.ItemDescription,
							Transactions = new ObservableCollection<StockTransaction>(ordered)
						};
					}));

			OnPropertyChanged(nameof(HasLedgerMismatch));
		}
		private void BuildRiskMatrix(List<ItemMovementSummary> items)
		{
			RiskMatrix.Clear();

			var zones = new[]
			{
		"High-High", "High-Medium", "High-Low",
		"Medium-High", "Medium-Medium", "Medium-Low",
		"Low-High", "Low-Medium", "Low-Low"
	};

			foreach (var zone in zones)
			{
				var count = items.Count(x => x.RiskHeatZone == zone);

				RiskMatrix.Add(new RiskMatrixCell
				{
					Label = zone,
					ItemCount = count,
					BackgroundColor = GetHeatColor(zone)
				});
			}
		}

		private string GetHeatColor(string zone)
		{
			if (zone.StartsWith("High-High")) return "#8B0000";
			if (zone.Contains("High")) return "#D32F2F";
			if (zone.Contains("Medium")) return "#F57C00";
			return "#388E3C";
		}

		private void EvaluateExecutiveRisk()
		{
			if (CriticalACount > 0)
			{
				ExecutiveRiskLevel = "CRITICAL-A RISK";
				ExecutiveRiskColor = "#C62828"; // Red
			}
			else if (HighRiskCount > 0)
			{
				ExecutiveRiskLevel = "HIGH RISK ITEMS";
				ExecutiveRiskColor = "#EF6C00"; // Orange
			}
			else if (MediumRiskCount > 0)
			{
				ExecutiveRiskLevel = "MEDIUM RISK ITEMS";
				ExecutiveRiskColor = "#F9A825"; // Yellow
			}
			else if (CapitalLockedPercentage > 15m)
			{
				ExecutiveRiskLevel = "CAPITAL LOCK WARNING";
				ExecutiveRiskColor = "#AD1457"; // Deep warning
			}
			else
			{
				ExecutiveRiskLevel = "SYSTEM HEALTHY";
				ExecutiveRiskColor = "#2E7D32"; // Green
			}
		}



		private void ExportExecutiveReport()
		{
			// Get fresh data directly from repository
			var allItems = _repository.GetAll();
			var allTransactions = _repository.GetAllTransactions();

			var analytics = _analyticsService.BuildAnalytics(allTransactions, allItems);

			var filePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				$"Executive_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf");

			var reportService = new ExecutiveReportService();

			reportService.GenerateExecutiveReport(
				analytics,
				App.ExecutiveSeverityOptions,
				filePath);

			MessageBox.Show(
				"Executive report generated successfully.",
				"Export Complete",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}
		private void ValidateRunningBalance(List<StockTransaction> transactions)
		{
			int runningBalance = 0;

			foreach (var tx in transactions)
			{
				runningBalance += tx.QtyIn;
				runningBalance -= tx.QtyOut;

				tx.CalculatedBalance = runningBalance;
				tx.IsBalanceMismatch = runningBalance != tx.BalanceAfter;
				tx.IsNegativeDrift = runningBalance < 0;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Services;

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

		[ObservableProperty] private int totalStockItems;
		[ObservableProperty] private int totalUnitsInStock;
		[ObservableProperty] private decimal totalInventoryValue;
		[ObservableProperty] private int totalTransactions;

		[ObservableProperty] private DateTime? startDate;
		[ObservableProperty] private DateTime? endDate;

		[ObservableProperty] private int periodTotalIn;
		[ObservableProperty] private int periodTotalOut;
		[ObservableProperty] private int periodNetMovement;
		[ObservableProperty] private decimal periodValueIn;
		[ObservableProperty] private decimal periodValueOut;
		[ObservableProperty] private decimal periodNetValue;

		[ObservableProperty] private string topMovingItem = "-";
		[ObservableProperty] private decimal topMovingItemValue;
		[ObservableProperty] private string mostConsumedItem = "-";
		[ObservableProperty] private int mostConsumedUnits;
		[ObservableProperty] private string highestGrowthItem = "-";
		[ObservableProperty] private int highestGrowthUnits;
		[ObservableProperty] private int deadStockCount;

		[ObservableProperty] private ObservableCollection<StockLedgerGroup> ledgerGroups = new();
		[ObservableProperty] private ObservableCollection<MonthlyMovementSummary> monthlySummaries = new();
		[ObservableProperty] private ObservableCollection<ItemMovementSummary> itemMovementSummaries = new();
		[ObservableProperty] private ObservableCollection<ItemMovementSummary> reorderAlerts = new();

		public bool HasLedgerMismatch =>
			LedgerGroups.Any(g =>
				g.Transactions.Any(t => t.IsBalanceMismatch));

		public StockLedgerViewModel()
		{
			_repository = new StockRepository();
			_analyticsService = new StockAnalyticsService();

			RecalculateCommand = new RelayCommand(RecalculateBalances);
			ApplyFilterCommand = new RelayCommand(LoadLedger);

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
			TotalInventoryValue = allItems.Sum(i => i.Quantity * i.AverageUnitCost);
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

			ReorderAlerts = new(
				analytics.ItemSummaries
					.Where(x =>
						x.ReorderRiskLevel == "Critical" ||
						x.ReorderRiskLevel == "High" ||
						x.ReorderRiskLevel == "Medium")
					.OrderBy(x => x.DaysUntilStockout)
			);

			TopMovingItem = analytics.TopMovingItem;
			TopMovingItemValue = analytics.TopMovingItemValue;
			MostConsumedItem = analytics.MostConsumedItem;
			MostConsumedUnits = analytics.MostConsumedUnits;
			HighestGrowthItem = analytics.HighestGrowthItem;
			HighestGrowthUnits = analytics.HighestGrowthUnits;
			DeadStockCount = analytics.DeadStockCount;

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
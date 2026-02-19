using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

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

		public IRelayCommand RecalculateCommand { get; }
		public IRelayCommand ApplyFilterCommand { get; }

		// GLOBAL SUMMARY
		[ObservableProperty] private int totalStockItems;
		[ObservableProperty] private int totalUnitsInStock;
		[ObservableProperty] private decimal totalInventoryValue;
		[ObservableProperty] private int totalTransactions;

		// DATE FILTER
		[ObservableProperty] private DateTime? startDate;
		[ObservableProperty] private DateTime? endDate;

		// PERIOD SUMMARY
		[ObservableProperty] private int periodTotalIn;
		[ObservableProperty] private int periodTotalOut;
		[ObservableProperty] private int periodNetMovement;
		[ObservableProperty] private decimal periodValueIn;
		[ObservableProperty] private decimal periodValueOut;
		[ObservableProperty] private decimal periodNetValue;

		// KPIs
		[ObservableProperty] private string topMovingItem = "-";
		[ObservableProperty] private decimal topMovingItemValue;
		[ObservableProperty] private string mostConsumedItem = "-";
		[ObservableProperty] private int mostConsumedUnits;
		[ObservableProperty] private string highestGrowthItem = "-";
		[ObservableProperty] private int highestGrowthUnits;
		[ObservableProperty] private int deadStockCount;

		[ObservableProperty]
		private ObservableCollection<StockLedgerGroup> ledgerGroups = new();

		[ObservableProperty]
		private ObservableCollection<MonthlyMovementSummary> monthlySummaries = new();

		[ObservableProperty]
		private ObservableCollection<ItemMovementSummary> itemMovementSummaries = new();


		public StockLedgerViewModel()
		{
			_repository = new StockRepository();
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

			// GLOBAL SUMMARY
			TotalStockItems = allItems.Count;
			TotalUnitsInStock = allItems.Sum(i => i.Quantity);
			TotalInventoryValue = allItems.Sum(i => i.Quantity * i.AverageUnitCost);
			TotalTransactions = allTransactions.Count;

			// DATE FILTER
			var filtered = allTransactions.AsEnumerable();

			if (StartDate.HasValue)
				filtered = filtered.Where(t => t.TransactionDate.Date >= StartDate.Value.Date);

			if (EndDate.HasValue)
				filtered = filtered.Where(t => t.TransactionDate.Date <= EndDate.Value.Date);

			var transactions = filtered
				.OrderBy(t => t.TransactionDate)
				.ThenBy(t => t.Id)
				.ToList();

			// PERIOD SUMMARY
			PeriodTotalIn = transactions.Sum(t => t.QtyIn);
			PeriodTotalOut = transactions.Sum(t => t.QtyOut);
			PeriodNetMovement = PeriodTotalIn - PeriodTotalOut;

			PeriodValueIn = transactions.Where(t => t.Type == "IN").Sum(t => t.TransactionValue);
			PeriodValueOut = transactions.Where(t => t.Type == "OUT").Sum(t => t.TransactionValue);
			PeriodNetValue = PeriodValueIn - PeriodValueOut;

			// EXECUTIVE MOVEMENT + TURNOVER (SAFE VERSION)

			const decimal periodDays = 30m;

			ItemMovementSummaries = new ObservableCollection<ItemMovementSummary>(
				transactions
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

						string category = turnover switch
						{
							> 5 => "Fast Moving",
							> 2 => "Healthy",
							> 0 => "Slow Moving",
							_ => "Dead Stock"
						};

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
					.ToList());

			// KPIs
			var topValueItem = ItemMovementSummaries.OrderByDescending(x => x.MovementValue).FirstOrDefault();
			if (topValueItem != null)
			{
				TopMovingItem = topValueItem.ItemCode;
				TopMovingItemValue = topValueItem.MovementValue;
			}

			var mostConsumed = ItemMovementSummaries.OrderByDescending(x => x.TotalOut).FirstOrDefault();
			if (mostConsumed != null)
			{
				MostConsumedItem = mostConsumed.ItemCode;
				MostConsumedUnits = mostConsumed.TotalOut;
			}

			var highestGrowth = ItemMovementSummaries.OrderByDescending(x => x.NetMovement).FirstOrDefault();
			if (highestGrowth != null)
			{
				HighestGrowthItem = highestGrowth.ItemCode;
				HighestGrowthUnits = highestGrowth.NetMovement;
			}

			DeadStockCount = ItemMovementSummaries.Count(x => x.TotalIn == 0 && x.TotalOut == 0);

			// LEDGER GROUPING
			LedgerGroups = new ObservableCollection<StockLedgerGroup>(
				transactions
					.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
					.OrderBy(g => g.Key.ItemCode)
					.Select(g => new StockLedgerGroup
					{
						ItemCode = g.Key.ItemCode,
						Description = g.Key.ItemDescription,
						Transactions = new ObservableCollection<StockTransaction>(
							g.OrderBy(t => t.TransactionDate)
							 .ThenBy(t => t.Id))
					}));

			// MONTHLY TREND
			MonthlySummaries = new ObservableCollection<MonthlyMovementSummary>(
				transactions
					.GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1))
					.OrderBy(g => g.Key)
					.Select(g => new MonthlyMovementSummary
					{
						Month = g.Key,
						TotalIn = g.Sum(x => x.QtyIn),
						TotalOut = g.Sum(x => x.QtyOut),
						ValueIn = g.Where(x => x.Type == "IN").Sum(x => x.TransactionValue),
						ValueOut = g.Where(x => x.Type == "OUT").Sum(x => x.TransactionValue)
					})
					.ToList());
		}
	}
}

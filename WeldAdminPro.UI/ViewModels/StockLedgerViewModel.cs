using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public class StockLedgerGroup
	{
		public string ItemCode { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public ObservableCollection<StockTransaction> Transactions { get; set; }
			= new();

		public int OpeningBalance =>
			Transactions.FirstOrDefault()?.BalanceAfter -
			(Transactions.FirstOrDefault()?.Type == "IN"
				? Transactions.FirstOrDefault()?.Quantity ?? 0
				: -(Transactions.FirstOrDefault()?.Quantity ?? 0)) ?? 0;

		public int TotalIn => Transactions.Sum(t => t.QtyIn);

		public int TotalOut => Transactions.Sum(t => t.QtyOut);

		public int NetMovement => TotalIn - TotalOut;

		public int CurrentBalance =>
			Transactions.LastOrDefault()?.BalanceAfter ?? 0;

		public decimal TotalMovementValue =>
			Transactions.Sum(t => t.TransactionValue);
	}


	public partial class StockLedgerViewModel : ObservableObject
	{
		private readonly StockRepository _repository;

		public IRelayCommand RecalculateCommand { get; }

		// 🔵 GLOBAL SUMMARY PROPERTIES (Correct Location)

		[ObservableProperty]
		private int totalStockItems;

		[ObservableProperty]
		private int totalUnitsInStock;

		[ObservableProperty]
		private decimal totalInventoryValue;

		[ObservableProperty]
		private int totalTransactions;

		[ObservableProperty]
		private DateTime? startDate;

		[ObservableProperty]
		private DateTime? endDate;

		public IRelayCommand ApplyFilterCommand { get; }


		private ObservableCollection<StockLedgerGroup> _ledgerGroups = new();
		public ObservableCollection<StockLedgerGroup> LedgerGroups
		{
			get => _ledgerGroups;
			set => SetProperty(ref _ledgerGroups, value);
		}

		public StockLedgerViewModel()
		{
			_repository = new StockRepository();

			RecalculateCommand = new RelayCommand(RecalculateBalances);
			ApplyFilterCommand = new RelayCommand(LoadLedger);

			LoadLedger();
		}


		private void RecalculateBalances()
		{
			var result = System.Windows.MessageBox.Show(
				"This will recalculate all stored balances and stock quantities.\n\nContinue?",
				"Confirm Recalculation",
				System.Windows.MessageBoxButton.YesNo,
				System.Windows.MessageBoxImage.Warning);

			if (result != System.Windows.MessageBoxResult.Yes)
				return;

			_repository.RecalculateAllBalances();

			LoadLedger();

			System.Windows.MessageBox.Show(
				"Recalculation complete.",
				"Success",
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Information);
		}

		private void LoadLedger()
		{
			var allItems = _repository.GetAll();
			var allTransactions = _repository.GetAllTransactions();

			// 🔵 Populate Global Summary
			TotalStockItems = allItems.Count;
			TotalUnitsInStock = allItems.Sum(i => i.Quantity);
			TotalInventoryValue = allItems.Sum(i => i.Quantity * i.AverageUnitCost);
			TotalTransactions = allTransactions.Count;

			var filteredTransactions = allTransactions.AsEnumerable();

			if (StartDate.HasValue)
				filteredTransactions = filteredTransactions
					.Where(t => t.TransactionDate.Date >= StartDate.Value.Date);

			if (EndDate.HasValue)
				filteredTransactions = filteredTransactions
					.Where(t => t.TransactionDate.Date <= EndDate.Value.Date);

			var transactions = filteredTransactions
				.OrderBy(t => t.TransactionDate)
				.ThenBy(t => t.Id)
				.ToList();


			var grouped = transactions
				.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
				.OrderBy(g => g.Key.ItemCode);

			var result = new ObservableCollection<StockLedgerGroup>();

			foreach (var group in grouped)
			{
				var ordered = group
					.OrderBy(t => t.TransactionDate)
					.ThenBy(t => t.Id)
					.ToList();

				if (!ordered.Any())
					continue;

				// Clear mismatch flags
				foreach (var tx in ordered)
					tx.IsLedgerMismatch = false;

				// Verify final balance integrity
				var finalBalance = ordered.Last().BalanceAfter;
				var actualQty = _repository.GetAvailableQuantity(group.Key.StockItemId);

				if (finalBalance != actualQty)
				{
					ordered.Last().IsLedgerMismatch = true;

					System.Diagnostics.Debug.WriteLine(
						$"Ledger integrity warning for item {group.Key.ItemCode}");
				}

				result.Add(new StockLedgerGroup
				{
					ItemCode = group.Key.ItemCode,
					Description = group.Key.ItemDescription,
					Transactions = new ObservableCollection<StockTransaction>(ordered)
				});
			}

			LedgerGroups = result;
		}
	}
}

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

		public int CurrentBalance =>
			Transactions.LastOrDefault()?.BalanceAfter ?? 0;
	}

	public partial class StockLedgerViewModel : ObservableObject
	{
		private readonly StockRepository _repository;

		public IRelayCommand RecalculateCommand { get; }

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
			var transactions = _repository
				.GetAllTransactions()
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

				// 🔹 Derive opening balance from first stored transaction
				int runningBalance = ordered.First().BalanceAfter;

				runningBalance -= ordered.First().Type == "IN"
					? ordered.First().Quantity
					: -ordered.First().Quantity;

				bool hasMismatch = false;

				foreach (var tx in ordered)
				{
					runningBalance += tx.Type == "IN"
						? tx.Quantity
						: -tx.Quantity;

					if (runningBalance != tx.BalanceAfter)
					{
						tx.IsLedgerMismatch = true;
						hasMismatch = true;
					}
					else
					{
						tx.IsLedgerMismatch = false;
					}
				}

				// 🔹 Final integrity check vs StockItems table
				var actualQty = _repository.GetAvailableQuantity(group.Key.StockItemId);

				if (runningBalance != actualQty)
				{
					hasMismatch = true;
					ordered.Last().IsLedgerMismatch = true;
				}

				if (hasMismatch)
				{
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

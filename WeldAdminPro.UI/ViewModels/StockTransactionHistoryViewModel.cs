using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionHistoryViewModel : ObservableObject
	{
		private readonly StockRepository _repository;

		// Master list (never filtered)
		private readonly ObservableCollection<StockTransaction> _allTransactions
			= new();

		public ObservableCollection<StockTransaction> Transactions { get; }
			= new();

		// =========================
		// FILTER DATA
		// =========================

		public ObservableCollection<string> ItemCodes { get; }
			= new();

		[ObservableProperty]
		private string selectedItemCode = "All";

		[ObservableProperty]
		private DateTime? fromDate;

		[ObservableProperty]
		private DateTime? toDate;

		public StockTransactionHistoryViewModel()
		{
			_repository = new StockRepository();

			Load();
		}

		// =========================
		// LOAD + BALANCE CALC
		// =========================
		private void Load()
		{
			_allTransactions.Clear();
			Transactions.Clear();
			ItemCodes.Clear();

			var raw = _repository.GetAllTransactions();

			// --- Running balance per Stock Item ---
			foreach (var group in raw.GroupBy(t => t.StockItemId))
			{
				int balance = 0;

				foreach (var tx in group.OrderBy(t => t.TransactionDate))
				{
					balance += tx.Type == "IN"
						? tx.Quantity
						: -tx.Quantity;

					tx.RunningBalance = balance;
				}
			}

			foreach (var tx in raw)
				_allTransactions.Add(tx);

			// Populate Item filter
			ItemCodes.Add("All");
			foreach (var code in raw
				.Select(t => t.ItemCode)
				.Distinct()
				.OrderBy(c => c))
			{
				ItemCodes.Add(code);
			}

			SelectedItemCode = "All";

			ApplyFilters();
		}

		// =========================
		// FILTER TRIGGERS
		// =========================
		partial void OnSelectedItemCodeChanged(string value) => ApplyFilters();
		partial void OnFromDateChanged(DateTime? value) => ApplyFilters();
		partial void OnToDateChanged(DateTime? value) => ApplyFilters();

		// =========================
		// FILTER LOGIC
		// =========================
		private void ApplyFilters()
		{
			Transactions.Clear();

			var filtered = _allTransactions.AsEnumerable();

			if (SelectedItemCode != "All")
				filtered = filtered.Where(t =>
					t.ItemCode == SelectedItemCode);

			if (FromDate.HasValue)
				filtered = filtered.Where(t =>
					t.TransactionDate.Date >= FromDate.Value.Date);

			if (ToDate.HasValue)
				filtered = filtered.Where(t =>
					t.TransactionDate.Date <= ToDate.Value.Date);

			foreach (var tx in filtered
				.OrderByDescending(t => t.TransactionDate))
			{
				Transactions.Add(tx);
			}
		}
	}
}

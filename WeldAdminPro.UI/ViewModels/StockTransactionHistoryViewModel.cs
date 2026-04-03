using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public class StockTransactionHistoryViewModel : ObservableObject
	{
		private readonly StockRepository _repo;

		private List<StockTransaction> _allTransactions = new();

		private ObservableCollection<StockTransaction> _transactions = new();
		public ObservableCollection<StockTransaction> Transactions
		{
			get => _transactions;
			set => SetProperty(ref _transactions, value);
		}

		private ObservableCollection<string> _itemCodes = new();
		public ObservableCollection<string> ItemCodes
		{
			get => _itemCodes;
			set => SetProperty(ref _itemCodes, value);
		}

		private string? _selectedItemCode;
		public string? SelectedItemCode
		{
			get => _selectedItemCode;
			set
			{
				if (SetProperty(ref _selectedItemCode, value))
					ApplyFilters();
			}
		}

		private DateTime? _fromDate;
		public DateTime? FromDate
		{
			get => _fromDate;
			set
			{
				if (SetProperty(ref _fromDate, value))
					ApplyFilters();
			}
		}

		private DateTime? _toDate;
		public DateTime? ToDate
		{
			get => _toDate;
			set
			{
				if (SetProperty(ref _toDate, value))
					ApplyFilters();
			}
		}

		public StockTransactionHistoryViewModel()
		{
			_repo = new StockRepository();
			Reload();
		}

		// =====================================================
		// PUBLIC RELOAD (CRITICAL FOR LIVE UPDATES)
		// =====================================================
		public void Reload()
		{
			LoadTransactions();
			BuildItemCodeList();
			ApplyFilters();
		}

		// =====================================================
		// LOAD FROM DATABASE
		// =====================================================
		private void LoadTransactions()
		{
			_allTransactions = _repo
				.GetAllTransactions()
				.OrderBy(t => t.TransactionDate)
				.ToList();

#if DEBUG
			foreach (var t in _allTransactions)
			{
				System.Diagnostics.Debug.WriteLine(
					$"Tx: {t.Id} | ProjectId: {t.ProjectId} | ProjectName: {t.ProjectName}");
			}
#endif
		}

		// =====================================================
		// BUILD ITEM FILTER LIST (NULL SAFE)
		// =====================================================
		private void BuildItemCodeList()
		{
			var codes = _allTransactions
				.Where(t => !string.IsNullOrWhiteSpace(t.ItemCode))
				.Select(t => t.ItemCode!)
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			codes.Insert(0, "All");

			ItemCodes = new ObservableCollection<string>(codes);

			// Null-safe validation
			if (string.IsNullOrWhiteSpace(SelectedItemCode) ||
				!codes.Contains(SelectedItemCode))
			{
				SelectedItemCode = "All";
			}
		}

		// =====================================================
		// FILTERING ENGINE
		// =====================================================
		private void ApplyFilters()
		{
			IEnumerable<StockTransaction> filtered = _allTransactions;

			// Filter by Item
			if (!string.IsNullOrWhiteSpace(SelectedItemCode) &&
				SelectedItemCode != "All")
			{
				filtered = filtered.Where(t => t.ItemCode == SelectedItemCode);
			}

			// Filter by From Date
			if (FromDate.HasValue)
			{
				filtered = filtered.Where(t =>
					t.TransactionDate >= FromDate.Value);
			}

			// Filter by To Date
			if (ToDate.HasValue)
			{
				filtered = filtered.Where(t =>
					t.TransactionDate <= ToDate.Value
						.AddDays(1)
						.AddSeconds(-1));
			}

			Transactions = new ObservableCollection<StockTransaction>(filtered);
		}
	}
}

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionHistoryViewModel : ObservableObject
	{
		private readonly StockTransactionRepository _repo = new();

		public StockItem Item { get; }

		[ObservableProperty]
		private ObservableCollection<StockTransaction> transactions = new();

		// =========================
		// CONSTRUCTOR (FIX)
		// =========================
		public StockTransactionHistoryViewModel(StockItem item)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Load();
		}

		// =========================
		// LOAD TRANSACTIONS
		// =========================
		private void Load()
		{
			Transactions.Clear();

			foreach (var tx in _repo.GetByStockItem(Item.Id))
				Transactions.Add(tx);
		}
	}
}

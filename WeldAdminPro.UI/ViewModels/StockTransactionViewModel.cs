using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionViewModel : ObservableObject
	{
		private readonly StockRepository _repo;
		private readonly StockTransactionRepository _txRepo;
		private readonly bool _isStockIn;

		public event Action? TransactionCompleted;
		public event Action? RequestClose;

		// =========================
		// BINDINGS
		// =========================
		public StockItem Item { get; }

		[ObservableProperty]
		private string quantityText = string.Empty;

		[ObservableProperty]
		private string reference = string.Empty;

		// =========================
		// CONSTRUCTOR
		// =========================
		public StockTransactionViewModel(StockItem item, bool isStockIn)
		{
			Item = item;
			_isStockIn = isStockIn;

			_repo = new StockRepository();
			_txRepo = new StockTransactionRepository();
		}

		// =========================
		// SAVE TRANSACTION
		// =========================
		[RelayCommand]
		private void Save()
		{
			if (!int.TryParse(QuantityText, out int qty) || qty <= 0)
			{
				MessageBox.Show("Quantity must be greater than zero.", "Validation Error");
				return;
			}

			if (!_isStockIn && Item.Quantity < qty)
			{
				MessageBox.Show("Insufficient stock for Stock OUT.", "Stock Error");
				return;
			}

			// =========================
			// APPLY STOCK CHANGE
			// =========================
			Item.Quantity += _isStockIn ? qty : -qty;
			_repo.Update(Item);

			// =========================
			// LOG TRANSACTION
			// =========================
			var transaction = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = Item.Id,
				TransactionDate = DateTime.Now,
				Quantity = qty,
				Type = _isStockIn ? "IN" : "OUT",
				Reference = Reference
			};

			_txRepo.Add(transaction);

			TransactionCompleted?.Invoke();
			RequestClose?.Invoke();
		}

		// =========================
		// CANCEL
		// =========================
		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionViewModel : ObservableObject
	{
		private readonly StockRepository _repo;
		private readonly bool _isStockIn;

		public StockItem Item { get; }

		[ObservableProperty]
		private string quantityText = string.Empty;

		[ObservableProperty]
		private string reference = string.Empty;

		public string Title => _isStockIn ? "Stock IN" : "Stock OUT";

		public IRelayCommand SaveCommand { get; }
		public IRelayCommand CancelCommand { get; }

		public event Action? TransactionCompleted;
		public event Action? RequestClose;

		public StockTransactionViewModel(StockItem item, bool isStockIn)
		{
			Item = item;
			_isStockIn = isStockIn;
			_repo = new StockRepository();

			SaveCommand = new RelayCommand(Save);
			CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
		}

		private void Save()
		{
			if (!int.TryParse(
				QuantityText,
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out var quantity) || quantity <= 0)
			{
				MessageBox.Show("Please enter a valid quantity greater than zero.");
				return;
			}

			// 🔐 Stock OUT safety
			if (!_isStockIn && quantity > Item.Quantity)
			{
				MessageBox.Show("Cannot stock out more than the available quantity.");
				return;
			}

			var tx = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = Item.Id,
				TransactionDate = DateTime.Now,
				Quantity = quantity,              // ✅ ALWAYS POSITIVE
				Type = _isStockIn ? "IN" : "OUT",  // ✅ Repository handles sign
				Reference = Reference
			};

			_repo.AddTransaction(tx);

			TransactionCompleted?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

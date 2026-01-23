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

		// String binding (safe for WPF)
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
			// 🔑 Parse quantity safely
			if (!int.TryParse(QuantityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
			{
				MessageBox.Show("Please enter a valid quantity greater than zero.");
				return;
			}

			// 🔑 Apply correct sign
			var signedQuantity = _isStockIn ? quantity : -quantity;

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
				Quantity = signedQuantity,
				Type = _isStockIn ? "IN" : "OUT",
				Reference = Reference
			};

			_repo.AddTransaction(tx);

			TransactionCompleted?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

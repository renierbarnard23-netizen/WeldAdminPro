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
		private string unitCostText = string.Empty;

		[ObservableProperty]
		private string reference = string.Empty;

		public string Title => _isStockIn ? "Stock IN" : "Stock OUT";
		public bool IsStockIn => _isStockIn;
		public bool IsStockOut => !_isStockIn;

		public IRelayCommand SaveCommand { get; }
		public IRelayCommand CancelCommand { get; }

		public event Action? TransactionCompleted;
		public event Action? RequestClose;

		public StockTransactionViewModel(StockItem item, bool isStockIn)
		{
			Item = item;
			_isStockIn = isStockIn;
			_repo = new StockRepository();

			if (_isStockIn)
				UnitCostText = item.AverageUnitCost.ToString("0.00", CultureInfo.CurrentCulture);

			SaveCommand = new RelayCommand(Save);
			CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
		}

		private void Save()
		{
			if (!int.TryParse(
				QuantityText,
				NumberStyles.Integer,
				CultureInfo.CurrentCulture,
				out var quantity) || quantity <= 0)
			{
				MessageBox.Show("Please enter a valid quantity greater than zero.");
				return;
			}

			decimal unitCost = 0;

			if (_isStockIn)
			{
				if (!decimal.TryParse(
					UnitCostText,
					NumberStyles.Number,
					CultureInfo.CurrentCulture,
					out unitCost) || unitCost < 0)
				{
					MessageBox.Show("Please enter a valid unit cost.");
					return;
				}
			}

			if (!_isStockIn && quantity > Item.Quantity)
			{
				MessageBox.Show("Cannot stock out more than the available quantity.");
				return;
			}

			var tx = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = Item.Id,
				ProjectId = null,
				TransactionDate = DateTime.Now,
				Quantity = quantity,
				Type = _isStockIn ? "IN" : "OUT",
				UnitCost = _isStockIn ? unitCost : Item.AverageUnitCost,
				Reference = Reference
			};

			_repo.AddTransaction(tx);

			TransactionCompleted?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

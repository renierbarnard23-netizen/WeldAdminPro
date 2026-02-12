using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockInViewModel : ObservableObject
	{
		private readonly StockRepository _repository;

		public ObservableCollection<StockItem> StockItems { get; }

		[ObservableProperty]
		private StockItem? selectedStockItem;

		[ObservableProperty]
		private int quantity;

		[ObservableProperty]
		private decimal unitCost;

		[ObservableProperty]
		private string reference = string.Empty;

		public event Action? RequestClose;

		public StockInViewModel()
		{
			_repository = new StockRepository();
			StockItems = new ObservableCollection<StockItem>(_repository.GetAll());
		}

		[RelayCommand]
		private void Save()
		{
			if (SelectedStockItem == null || Quantity <= 0)
				return;

			var tx = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = SelectedStockItem.Id,
				Quantity = Quantity,
				Type = "IN",
				UnitCost = UnitCost,
				TransactionDate = DateTime.UtcNow,
				Reference = Reference
			};

			_repository.AddTransaction(tx);

			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}

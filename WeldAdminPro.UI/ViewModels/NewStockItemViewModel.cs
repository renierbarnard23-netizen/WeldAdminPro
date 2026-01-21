using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class NewStockItemViewModel : ObservableObject
	{
		private readonly StockRepository _repo = new();

		private readonly bool _isEditMode;

		public event Action? ItemCreated;
		public event Action? RequestClose;

		[ObservableProperty]
		private StockItem item;

		public ObservableCollection<string> Categories { get; } =
			new()
			{
				"Electrodes",
				"Gas",
				"Abrasives",
				"PPE",
				"Medical",
				"Uncategorised"
			};

		// =========================
		// NEW ITEM
		// =========================
		public NewStockItemViewModel()
		{
			_isEditMode = false;

			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Quantity = 0,
				Category = "Uncategorised"
			};
		}

		// =========================
		// EDIT ITEM
		// =========================
		public NewStockItemViewModel(StockItem existing)
		{
			_isEditMode = true;
			Item = existing;
		}

		// =========================
		// SAVE
		// =========================
		[RelayCommand]
		private void Save()
		{
			if (_isEditMode)
			{
				_repo.Update(Item);
			}
			else
			{
				_repo.Add(Item);
			}

			ItemCreated?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

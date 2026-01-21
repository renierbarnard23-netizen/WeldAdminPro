using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class NewStockItemViewModel : ObservableObject
	{
		private readonly StockRepository _repo = new();

		public event Action? ItemCreated;
		public event Action? RequestClose;

		[ObservableProperty]
		private StockItem item = null!;

		public ObservableCollection<string> Categories { get; } =
			new()
			{
				"Electrodes",
				"Gas",
				"Abrasives",
				"PPE"
			};

		// NEW ITEM
		public NewStockItemViewModel()
		{
			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Quantity = 0,
				Category = "Uncategorised"
			};
		}

		// EDIT ITEM
		public NewStockItemViewModel(StockItem existing)
		{
			Item = existing;
			Item.Category ??= "Uncategorised";
		}

		[RelayCommand]
		private void Save()
		{
			// New item = not yet in DB
			if (_repo.GetAll().All(i => i.Id != Item.Id))
				_repo.Add(Item);
			else
				_repo.Update(Item);

			ItemCreated?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

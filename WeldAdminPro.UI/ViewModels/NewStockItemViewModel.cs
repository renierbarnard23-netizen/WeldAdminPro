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
		private readonly StockRepository _stockRepo = new();
		private readonly CategoryRepository _categoryRepo = new();

		public event Action? ItemCreated;
		public event Action? RequestClose;

		// =========================
		// Stock item
		// =========================

		[ObservableProperty]
		private StockItem item;

		// =========================
		// Categories (dynamic)
		// =========================

		[ObservableProperty]
		private ObservableCollection<string> categories = new();

		// =========================
		// Constructors
		// =========================

		public NewStockItemViewModel()
		{
			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Category = "Uncategorised"
			};

			LoadCategories();
		}

		public NewStockItemViewModel(StockItem existing)
		{
			Item = existing;
			LoadCategories();
		}
		public void RefreshCategories()
		{
			Categories.Clear();

			foreach (var cat in _categoryRepo.GetAllActive())
			{
				Categories.Add(cat.Name);
			}

			// Safety fallback
			if (!Categories.Contains(Item.Category))
				Item.Category = "Uncategorised";
		}


		// =========================
		// Load categories
		// =========================

		private void LoadCategories()
		{
			Categories.Clear();

			foreach (var cat in _categoryRepo.GetAllActive())
			{
				Categories.Add(cat.Name);
			}

			// Safety fallback
			if (!Categories.Contains(Item.Category))
				Item.Category = "Uncategorised";
		}

		// =========================
		// Save
		// =========================

		[RelayCommand]
		private void Save()
		{
			if (Item.Id == Guid.Empty)
				_stockRepo.Add(Item);
			else
				_stockRepo.Update(Item);

			ItemCreated?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

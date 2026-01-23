using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class NewStockItemViewModel : ObservableObject
	{
		private readonly StockRepository _stockRepo = new();
		private readonly CategoryRepository _categoryRepo = new();

		// =========================
		// Properties
		// =========================

		[ObservableProperty]
		private StockItem item = null!;

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		public bool IsEditMode { get; }

		public event Action? ItemCreated;
		public event Action? RequestClose;

		// =========================
		// NEW item
		// =========================
		public NewStockItemViewModel()
		{
			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Quantity = 0,
				Category = "Uncategorised"
			};

			LoadCategories();
			SelectedCategory = Categories.FirstOrDefault(c => c.Name == "Uncategorised");

			IsEditMode = false;
		}

		// =========================
		// EDIT item
		// =========================
		public NewStockItemViewModel(StockItem existingItem)
		{
			Item = new StockItem
			{
				Id = existingItem.Id,
				ItemCode = existingItem.ItemCode,
				Description = existingItem.Description,
				Quantity = existingItem.Quantity,
				Unit = existingItem.Unit,
				Category = existingItem.Category
			};

			LoadCategories();
			SelectedCategory =
				Categories.FirstOrDefault(c => c.Name == Item.Category)
				?? Categories.FirstOrDefault(c => c.Name == "Uncategorised");

			IsEditMode = true;
		}

		// =========================
		// Load categories
		// =========================
		private void LoadCategories()
		{
			Categories = new ObservableCollection<Category>(
				_categoryRepo.GetAllActive()
			);
		}

		// =========================
		// Commands
		// =========================
		[RelayCommand]
		private void Save()
		{
			// 🔑 CRITICAL LINE
			Item.Category = SelectedCategory?.Name ?? "Uncategorised";

			if (IsEditMode)
				_stockRepo.Update(Item);
			else
				_stockRepo.Add(Item);

			ItemCreated?.Invoke();
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}

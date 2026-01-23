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
		private readonly StockRepository _repo = new();
		private readonly CategoryRepository _categoryRepo = new();

		[ObservableProperty]
		private StockItem item;

		// =========================
		// Categories
		// =========================
		public ObservableCollection<Category> Categories { get; } = new();

		private Category? _selectedCategory;
		public Category? SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				SetProperty(ref _selectedCategory, value);
				Item.Category = value?.Name ?? "Uncategorised";
			}
		}

		public bool IsEditMode { get; }

		public event Action? ItemCreated;
		public event Action? RequestClose;

		// =========================
		// NEW item constructor
		// =========================
		public NewStockItemViewModel()
		{
			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Quantity = 0,
				Category = "Uncategorised"
			};

			IsEditMode = false;

			LoadCategories();
		}

		// =========================
		// EDIT item constructor
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
				Category = string.IsNullOrWhiteSpace(existingItem.Category)
					? "Uncategorised"
					: existingItem.Category
			};

			IsEditMode = true;

			LoadCategories();
		}

		// =========================
		// Load categories safely
		// =========================
		private void LoadCategories()
		{
			Categories.Clear();

			foreach (var c in _categoryRepo.GetAllActive())
				Categories.Add(c);

			// Select existing or default category
			SelectedCategory =
				Categories.FirstOrDefault(c => c.Name == Item.Category)
				?? Categories.FirstOrDefault(c => c.Name == "Uncategorised")
				?? Categories.FirstOrDefault();
		}

		// =========================
		// Commands
		// =========================
		[RelayCommand]
		private void Save()
		{
			if (IsEditMode)
				_repo.Update(Item);
			else
				_repo.Add(Item);

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

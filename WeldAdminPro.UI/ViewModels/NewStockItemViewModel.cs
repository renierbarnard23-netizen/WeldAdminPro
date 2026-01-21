using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class NewStockItemViewModel : ObservableObject
	{
		private readonly StockRepository _repo = new();
		private readonly CategoryRepository _categoryRepo = new();

		// =========================
		// STATE
		// =========================
		private readonly bool _isEditMode;

		public bool IsEditMode => _isEditMode;

		public event Action? ItemCreated;
		public event Action? RequestClose;

		// =========================
		// BINDINGS
		// =========================
		[ObservableProperty]
		private StockItem item = null!;

		[ObservableProperty]
		private ObservableCollection<string> categories = new();

		// =========================
		// NEW ITEM CONSTRUCTOR
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

			LoadCategories();
		}

		// =========================
		// EDIT ITEM CONSTRUCTOR
		// =========================
		public NewStockItemViewModel(StockItem existing)
		{
			_isEditMode = true;
			Item = existing;

			LoadCategories();
		}

		// =========================
		// CATEGORY LOAD
		// =========================
		private void LoadCategories()
		{
			Categories.Clear();

			foreach (var cat in _categoryRepo.GetAllActive())
				Categories.Add(cat.Name);

			if (string.IsNullOrWhiteSpace(Item.Category) ||
				!Categories.Contains(Item.Category))
			{
				Item.Category = "Uncategorised";
			}
		}

		// =========================
		// SAVE WITH VALIDATION
		// =========================
		[RelayCommand]
		private void Save()
		{
			// -------- Validation --------

			if (string.IsNullOrWhiteSpace(Item.ItemCode))
			{
				MessageBox.Show("Item Code is required.", "Validation Error");
				return;
			}

			if (string.IsNullOrWhiteSpace(Item.Description))
			{
				MessageBox.Show("Description is required.", "Validation Error");
				return;
			}

			if (Item.Quantity < 0)
			{
				MessageBox.Show("Quantity cannot be negative.", "Validation Error");
				return;
			}

			if (string.IsNullOrWhiteSpace(Item.Category))
				Item.Category = "Uncategorised";

			// -------- Duplicate Code Check --------

			var existingCodes = _repo.GetAll()
				.Where(i => i.Id != Item.Id)
				.Select(i => i.ItemCode)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			if (existingCodes.Contains(Item.ItemCode))
			{
				MessageBox.Show(
					"An item with this Item Code already exists.",
					"Duplicate Item Code",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			// -------- Persist --------

			if (_isEditMode)
				_repo.Update(Item);
			else
				_repo.Add(Item);

			ItemCreated?.Invoke();
			RequestClose?.Invoke();
		}
	}
}

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

		private readonly bool _isEditMode;

		public event Action? ItemCreated;
		public event Action? RequestClose;

		[ObservableProperty]
		private StockItem item = null!;

		[ObservableProperty]
		private ObservableCollection<string> categories = new();

		// =========================
		// VALIDATION : MIN / MAX (PER ITEM)
		// =========================
		private bool ValidateMinMaxLevels()
		{
			if (Item.MinLevel < 0)
			{
				MessageBox.Show(
					"Minimum stock level cannot be negative.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return false;
			}

			if (Item.MaxLevel < 0)
			{
				MessageBox.Show(
					"Maximum stock level cannot be negative.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return false;
			}

			if (Item.MaxLevel > 0 && Item.MaxLevel < Item.MinLevel)
			{
				MessageBox.Show(
					"Maximum stock level cannot be less than the minimum level.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return false;
			}

			return true;
		}

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
				MinLevel = 0,   // per-item minimum
				MaxLevel = 0,   // per-item maximum
				Category = "Uncategorised"
			};

			LoadCategories();
		}

		// =========================
		// EDIT ITEM
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

			if (!Categories.Contains(Item.Category))
				Categories.Add(Item.Category);
		}

		// =========================
		// SAVE
		// =========================
		[RelayCommand]
		private void Save()
		{
			// -------- Validation --------
			if (string.IsNullOrWhiteSpace(Item.ItemCode))
			{
				MessageBox.Show(
					"Item Code is required.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(Item.Description))
			{
				MessageBox.Show(
					"Description is required.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			if (Item.Quantity < 0)
			{
				MessageBox.Show(
					"Quantity cannot be negative.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			// 👉 Min / Max validation (PER ITEM)
			if (!ValidateMinMaxLevels())
				return;

			if (string.IsNullOrWhiteSpace(Item.Category))
				Item.Category = "Uncategorised";

			// -------- Ensure Category Exists --------
			_categoryRepo.Add(Item.Category);

			// -------- Duplicate Item Code --------
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

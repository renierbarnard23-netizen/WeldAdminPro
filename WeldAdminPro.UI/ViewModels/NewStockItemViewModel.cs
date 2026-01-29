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
		private readonly StockRepository _stockRepo = new();
		private readonly CategoryRepository _categoryRepo = new();

		[ObservableProperty]
		private StockItem item = null!;

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		public bool IsEditMode { get; }

		// 🔒 Immutable snapshot for edits
		private readonly string _originalItemCode = string.Empty;

		public event Action? ItemCreated;
		public event Action? RequestClose;

		// =========================
		// NEW ITEM
		// =========================
		public NewStockItemViewModel()
		{
			Item = new StockItem
			{
				Id = Guid.NewGuid(),
				Quantity = 0,
				Category = "Uncategorised",
				ItemCode = _stockRepo.GetNextItemCodeSuggestion()
			};

			LoadCategories();
			SelectedCategory = Categories.FirstOrDefault(c => c.Name == "Uncategorised");

			IsEditMode = false;
		}

		// =========================
		// EDIT ITEM
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
				MinLevel = existingItem.MinLevel,
				MaxLevel = existingItem.MaxLevel,
				Category = existingItem.Category
			};

			_originalItemCode = existingItem.ItemCode;

			LoadCategories();
			SelectedCategory =
				Categories.FirstOrDefault(c => c.Name == Item.Category)
				?? Categories.FirstOrDefault(c => c.Name == "Uncategorised");

			IsEditMode = true;
		}

		private void LoadCategories()
		{
			Categories = new ObservableCollection<Category>(
				_categoryRepo.GetAllActive()
			);
		}

		// =========================
		// SAVE
		// =========================
		[RelayCommand]
		private void Save()
		{
			// 🔒 Lock ItemCode in edit mode
			if (IsEditMode && Item.ItemCode != _originalItemCode)
			{
				MessageBox.Show(
					"Item Code cannot be changed once the item has been created.",
					"Item Code Locked",
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);

				Item.ItemCode = _originalItemCode;
				return;
			}

			// ✅ Min / Max validation
			if (Item.MinLevel.HasValue && Item.MaxLevel.HasValue)
			{
				if (Item.MinLevel.Value > Item.MaxLevel.Value)
				{
					MessageBox.Show(
						"Minimum level cannot be greater than Maximum level.",
						"Invalid Stock Levels",
						MessageBoxButton.OK,
						MessageBoxImage.Warning
					);
					return;
				}
			}

			Item.Category = SelectedCategory?.Name ?? "Uncategorised";

			try
			{
				if (IsEditMode)
					_stockRepo.Update(Item);
				else
					_stockRepo.Add(Item);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

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

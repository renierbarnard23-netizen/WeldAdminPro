using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class CategoryManagementViewModel : ObservableObject
	{
		private readonly CategoryRepository _repo = new();

		// =========================
		// BINDINGS
		// =========================
		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(ToggleCategoryText))]
		[NotifyPropertyChangedFor(nameof(CanToggleCategory))]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = string.Empty;

		[ObservableProperty]
		private string renameCategoryName = string.Empty;

		// =========================
		// INIT
		// =========================
		public CategoryManagementViewModel()
		{
			Load();
		}

		private void Load()
		{
			Categories.Clear();

			// 🔑 Load ALL categories (active + inactive)
			foreach (var cat in _repo.GetAll())
				Categories.Add(cat);
		}

		// =========================
		// ADD CATEGORY
		// =========================
		[RelayCommand]
		private void AddCategory()
		{
			if (string.IsNullOrWhiteSpace(NewCategoryName))
				return;

			var name = NewCategoryName.Trim();

			// Add OR re-enable
			_repo.Add(name);

			Load(); // authoritative refresh

			NewCategoryName = string.Empty;
		}

		// =========================
		// TOGGLE ACTIVE (ENABLE / DISABLE)
		// =========================
		[RelayCommand]
		private void ToggleActive()
		{
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.Name == "Uncategorised")
			{
				MessageBox.Show(
					"The 'Uncategorised' category cannot be disabled.",
					"Action Not Allowed",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			if (SelectedCategory.IsActive)
			{
				_repo.Disable(SelectedCategory.Id, SelectedCategory.Name);
			}
			else
			{
				_repo.Add(SelectedCategory.Name); // re-enable
			}

			Load();
		}

		// =========================
		// RENAME CATEGORY
		// =========================
		[RelayCommand]
		private void RenameCategory()
		{
			if (SelectedCategory == null)
				return;

			if (string.IsNullOrWhiteSpace(RenameCategoryName))
				return;

			_repo.Rename(SelectedCategory.Id, RenameCategoryName.Trim());

			RenameCategoryName = string.Empty;
			Load();
		}

		// =========================
		// UI HELPERS
		// =========================
		public string ToggleCategoryText =>
			SelectedCategory == null
				? "Enable / Disable Category"
				: SelectedCategory.IsActive
					? "Disable Category"
					: "Enable Category";

		public bool CanToggleCategory => SelectedCategory != null;
	}
}

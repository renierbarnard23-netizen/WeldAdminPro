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

			foreach (var cat in _repo.GetAllActive())
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

			// Persist to database
			_repo.Add(name);

			// Immediately update UI
			Categories.Add(new Category
			{
				Id = Guid.NewGuid(),
				Name = name,
				IsActive = true
			});

			NewCategoryName = string.Empty;
		}
		private void ToggleActive()
		{
			if (SelectedCategory == null)
				return;

			if (!SelectedCategory.IsActive)
			{
				_repo.Add(SelectedCategory.Name); // re-enable
			}
			else
			{
				_repo.Disable(SelectedCategory.Id, SelectedCategory.Name);
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
		// DISABLE CATEGORY
		// =========================
		[RelayCommand]
		private void DisableCategory()
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

			_repo.Disable(SelectedCategory.Id, SelectedCategory.Name);
			Load();
		}
	}
}

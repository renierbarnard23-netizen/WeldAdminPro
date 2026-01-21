using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class CategoryManagementViewModel : ObservableObject
	{
		private readonly CategoryRepository _repo = new();

		// =========================
		// Observable properties
		// =========================

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = "";

		[ObservableProperty]
		private string renameCategoryName = "";

		// =========================
		// Constructor
		// =========================

		public CategoryManagementViewModel()
		{
			LoadCategories();
		}

		// =========================
		// Load
		// =========================

		private void LoadCategories()
		{
			Categories = new ObservableCollection<Category>(_repo.GetAll());
		}

		// =========================
		// Commands
		// =========================

		[RelayCommand]
		private void AddCategory()
		{
			if (string.IsNullOrWhiteSpace(NewCategoryName))
				return;

			_repo.Add(NewCategoryName.Trim());

			NewCategoryName = "";
			LoadCategories();
		}

		[RelayCommand]
		private void RenameCategory()
		{
			if (SelectedCategory == null)
				return;

			if (string.IsNullOrWhiteSpace(RenameCategoryName))
				return;

			_repo.Rename(SelectedCategory.Id, RenameCategoryName.Trim());

			RenameCategoryName = "";
			LoadCategories();
		}

		[RelayCommand]
		private void DisableCategory()
		{
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.Name == "Uncategorised")
			{
				MessageBox.Show(
					"The 'Uncategorised' category cannot be disabled.",
					"Operation not allowed",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			_repo.SetActive(SelectedCategory.Id, false);
			LoadCategories();
		}

		[RelayCommand]
		private void EnableCategory()
		{
			if (SelectedCategory == null)
				return;

			_repo.SetActive(SelectedCategory.Id, true);
			LoadCategories();
		}
	}
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class CategoryManagementViewModel : ObservableObject
	{
		private readonly CategoryRepository _repo = new();

		// =========================
		// Properties
		// =========================

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = string.Empty;

		// =========================
		// Constructor
		// =========================

		public CategoryManagementViewModel()
		{
			LoadCategories();
		}

		private void LoadCategories()
		{
			Categories = new ObservableCollection<Category>(_repo.GetAll());
			SelectedCategory = null;
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
			NewCategoryName = string.Empty;

			LoadCategories();
		}

		[RelayCommand]
		private void DisableCategory()
		{
			if (SelectedCategory == null)
				return;

			try
			{
				_repo.Disable(SelectedCategory);
				LoadCategories();
			}
			catch (InvalidOperationException ex)
			{
				MessageBox.Show(
					ex.Message,
					"Category In Use",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}

		[RelayCommand]
		private void EnableCategory()
		{
			if (SelectedCategory == null)
				return;

			SelectedCategory.IsActive = true;
			_repo.Update(SelectedCategory);

			LoadCategories();
		}

		[RelayCommand]
		private void DeleteCategory()
		{
			if (SelectedCategory == null)
				return;

			try
			{
				_repo.Delete(SelectedCategory);
				LoadCategories();
			}
			catch (InvalidOperationException ex)
			{
				MessageBox.Show(
					ex.Message,
					"Category In Use",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}
	}
}

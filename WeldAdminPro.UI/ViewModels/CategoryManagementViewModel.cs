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

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = "";

		[ObservableProperty]
		private string renameCategoryName = "";

		public CategoryManagementViewModel()
		{
			Load();
		}

		private void Load()
		{
			Categories = new ObservableCollection<Category>(_repo.GetAllActive());
		}

		[RelayCommand]
		private void Add()
		{
			if (string.IsNullOrWhiteSpace(NewCategoryName))
				return;

			_repo.Add(NewCategoryName.Trim());
			NewCategoryName = "";
			Load();
		}

		[RelayCommand]
		private void Rename()
		{
			if (SelectedCategory == null || string.IsNullOrWhiteSpace(RenameCategoryName))
				return;

			_repo.Rename(SelectedCategory.Id, RenameCategoryName.Trim());
			RenameCategoryName = "";
			Load();
		}

		[RelayCommand]
		private void Disable()
		{
			if (SelectedCategory == null)
				return;

			try
			{
				_repo.Disable(SelectedCategory.Id, SelectedCategory.Name);
				Load();
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

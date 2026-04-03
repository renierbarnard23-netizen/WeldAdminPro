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

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = string.Empty;

		public CategoryManagementViewModel()
		{
			Load();
		}

		private void Load()
		{
			Categories = new ObservableCollection<Category>(_repo.GetAll());
			SelectedCategory = null;
		}

		[RelayCommand]
		private void Add()
		{
			if (string.IsNullOrWhiteSpace(NewCategoryName))
				return;

			_repo.Add(NewCategoryName.Trim());
			NewCategoryName = string.Empty;
			Load();
		}

		[RelayCommand]
		private void Enable()
		{
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.Name == "Uncategorised")
			{
				MessageBox.Show("The 'Uncategorised' category cannot be disabled.");
				return;
			}

			_repo.SetActive(SelectedCategory.Id, true);
			Load();
		}

		[RelayCommand]
		private void Disable()
		{
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.Name == "Uncategorised")
			{
				MessageBox.Show("The 'Uncategorised' category cannot be disabled.");
				return;
			}

			if (_repo.IsCategoryInUse(SelectedCategory.Name))
			{
				MessageBox.Show(
					"This category is currently assigned to stock items and cannot be disabled.",
					"Category In Use",
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
				return;
			}

			_repo.SetActive(SelectedCategory.Id, false);
			Load();
		}

		[RelayCommand]
		private void Delete()
		{
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.Name == "Uncategorised")
			{
				MessageBox.Show("The 'Uncategorised' category cannot be deleted.");
				return;
			}

			if (_repo.IsCategoryInUse(SelectedCategory.Name))
			{
				MessageBox.Show(
					"This category is currently assigned to stock items and cannot be deleted.",
					"Category In Use",
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
				return;
			}

			_repo.Delete(SelectedCategory.Id);
			Load();
		}
	}
}

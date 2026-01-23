using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class CategoryManagementViewModel : ObservableObject
	{
		private readonly CategoryRepository _repo = new();

		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private string newCategoryName = "";

		public CategoryManagementViewModel()
		{
			Load();
		}

		private void Load()
		{
			Categories.Clear();
			foreach (var c in _repo.GetAll())
				Categories.Add(c);
		}

		[RelayCommand]
		private void Add()
		{
			if (string.IsNullOrWhiteSpace(NewCategoryName))
				return;

			_repo.Add(NewCategoryName);
			NewCategoryName = "";
			Load();
		}

		[RelayCommand(CanExecute = nameof(HasSelection))]
		private void Enable()
		{
			_repo.SetActive(SelectedCategory!.Id, true);
			Load();
		}

		[RelayCommand(CanExecute = nameof(HasSelection))]
		private void Disable()
		{
			_repo.SetActive(SelectedCategory!.Id, false);
			Load();
		}

		[RelayCommand(CanExecute = nameof(CanDelete))]
		private void Delete()
		{
			if (MessageBox.Show("Delete category?",
				"Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				return;

			_repo.Delete(SelectedCategory!.Id);
			Load();
		}

		private bool HasSelection() => SelectedCategory != null;
		private bool CanDelete() => SelectedCategory != null &&
			SelectedCategory.Name != "Uncategorised";
	}
}

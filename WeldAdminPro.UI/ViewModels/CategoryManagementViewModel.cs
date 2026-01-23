using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

		public CategoryManagementViewModel()
		{
			LoadCategories();
		}

		private void LoadCategories()
		{
			Categories.Clear();
			foreach (var c in _repo.GetAll())
				Categories.Add(c);
		}

		[RelayCommand(CanExecute = nameof(CanToggle))]
		private void Disable()
		{
			if (SelectedCategory == null)
				return;

			_repo.Disable(SelectedCategory.Id);
			LoadCategories();
		}

		[RelayCommand(CanExecute = nameof(CanToggle))]
		private void Enable()
		{
			if (SelectedCategory == null)
				return;

			_repo.Enable(SelectedCategory.Id);
			LoadCategories();
		}

		private bool CanToggle() => SelectedCategory != null;
	}
}

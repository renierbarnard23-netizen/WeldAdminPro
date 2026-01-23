using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockViewModel : ObservableObject
	{
		private readonly StockRepository _stockRepo;
		private readonly CategoryRepository _categoryRepo;

		// =========================
		// Observable Properties
		// =========================

		[ObservableProperty]
		private ObservableCollection<StockItem> items = new();

		[ObservableProperty]
		private StockItem? selectedItem;

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		// =========================
		// Commands
		// =========================

		public IRelayCommand NewItemCommand { get; }
		public IRelayCommand EditItemCommand { get; }
		public IRelayCommand StockInCommand { get; }
		public IRelayCommand StockOutCommand { get; }

		// =========================
		// Constructor
		// =========================

		public StockViewModel()
		{
			_stockRepo = new StockRepository();
			_categoryRepo = new CategoryRepository();

			LoadCategories();
			LoadItems();

			NewItemCommand = new RelayCommand(OpenNewItem);
			EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
			StockInCommand = new RelayCommand(OpenStockIn, () => SelectedItem != null);
			StockOutCommand = new RelayCommand(OpenStockOut, () => SelectedItem != null);
		}

		// =========================
		// Category handling
		// =========================

		private void LoadCategories()
		{
			Categories.Clear();

			// Virtual "All" category
			Categories.Add(new Category
			{
				Id = Guid.Empty,
				Name = "All",
				IsActive = true
			});

			foreach (var cat in _categoryRepo.GetAllActive())
				Categories.Add(cat);

			SelectedCategory = Categories.FirstOrDefault();
		}

		partial void OnSelectedCategoryChanged(Category? value)
		{
			LoadItems();
		}

		// =========================
		// Selection handling
		// =========================

		partial void OnSelectedItemChanged(StockItem? value)
		{
			EditItemCommand.NotifyCanExecuteChanged();
			StockInCommand.NotifyCanExecuteChanged();
			StockOutCommand.NotifyCanExecuteChanged();
		}

		// =========================
		// Data loading
		// =========================

		private void LoadItems()
		{
			SelectedItem = null;

			var allItems = _stockRepo.GetAll();

			if (SelectedCategory == null || SelectedCategory.Name == "All")
			{
				Items = new ObservableCollection<StockItem>(allItems);
			}
			else
			{
				Items = new ObservableCollection<StockItem>(
					allItems.Where(i => i.Category == SelectedCategory.Name)
				);
			}
		}

		// =========================
		// 🔑 REQUIRED BY StockView.xaml.cs
		// =========================

		public void RefreshAfterCategoryChange()
		{
			LoadCategories();
			LoadItems();
		}

		// =========================
		// Stock actions
		// =========================

		private void OpenNewItem()
		{
			var vm = new NewStockItemViewModel();

			var window = new NewStockItemWindow(vm)
			{
				Owner = Application.Current.MainWindow,
				Title = "New Stock Item"
			};

			vm.ItemCreated += RefreshAfterCategoryChange;
			vm.RequestClose += window.Close;

			window.ShowDialog();
		}

		private void OpenEditItem()
		{
			if (SelectedItem == null)
				return;

			var vm = new NewStockItemViewModel(SelectedItem);

			var window = new NewStockItemWindow(vm)
			{
				Owner = Application.Current.MainWindow,
				Title = "Edit Stock Item"
			};

			vm.ItemCreated += RefreshAfterCategoryChange;
			vm.RequestClose += window.Close;

			window.ShowDialog();
		}

		private void OpenStockIn() => OpenTransaction(true);
		private void OpenStockOut() => OpenTransaction(false);

		private void OpenTransaction(bool isStockIn)
		{
			if (SelectedItem == null)
				return;

			var vm = new StockTransactionViewModel(SelectedItem, isStockIn);

			var window = new StockTransactionWindow(vm)
			{
				Owner = Application.Current.MainWindow
			};

			vm.TransactionCompleted += LoadItems;
			vm.RequestClose += window.Close;

			window.ShowDialog();
		}
	}
}

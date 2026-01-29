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

		[ObservableProperty]
		private ObservableCollection<StockItem> items = new();

		[ObservableProperty]
		private StockItem? selectedItem;

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		// 🔔 STATUS COUNTERS (Phase 9.4)
		[ObservableProperty]
		private int lowStockCount;

		[ObservableProperty]
		private int outOfStockCount;

		public bool HasStockWarnings => LowStockCount > 0 || OutOfStockCount > 0;

		public IRelayCommand NewItemCommand { get; }
		public IRelayCommand EditItemCommand { get; }
		public IRelayCommand StockInCommand { get; }
		public IRelayCommand StockOutCommand { get; }
		public IRelayCommand ViewHistoryCommand { get; }

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
			ViewHistoryCommand = new RelayCommand(OpenHistory);
		}

		private void LoadCategories()
		{
			Categories.Clear();

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

		partial void OnSelectedItemChanged(StockItem? value)
		{
			EditItemCommand.NotifyCanExecuteChanged();
			StockInCommand.NotifyCanExecuteChanged();
			StockOutCommand.NotifyCanExecuteChanged();
		}

		private void LoadItems()
		{
			SelectedItem = null;

			var allItems = _stockRepo.GetAll();

			var filtered = (SelectedCategory == null || SelectedCategory.Name == "All")
				? allItems
				: allItems.Where(i => i.Category == SelectedCategory.Name);

			Items = new ObservableCollection<StockItem>(filtered);

			RecalculateStatusCounters();
		}

		private void RecalculateStatusCounters()
		{
			OutOfStockCount = Items.Count(i => i.Status == StockStatus.Out);
			LowStockCount = Items.Count(i => i.Status == StockStatus.Low);

			OnPropertyChanged(nameof(HasStockWarnings));
		}

		public void RefreshAfterCategoryChange()
		{
			LoadCategories();
			LoadItems();
		}

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

		private void OpenHistory()
		{
			var window = new Window
			{
				Title = "Stock Transaction History",
				Content = new StockTransactionHistoryView(),
				Owner = Application.Current.MainWindow,
				Width = 900,
				Height = 600
			};

			window.ShowDialog();
		}
	}
}

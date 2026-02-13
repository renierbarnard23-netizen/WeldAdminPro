using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
	public enum StockStatusFilter
	{
		All,
		Low,
		Out
	}

	public partial class StockViewModel : ObservableObject
	{
		private const string LastStatusFilterKey = "LastStockStatusFilter";

		private readonly StockRepository _stockRepo;
		private readonly CategoryRepository _categoryRepo;

		private ObservableCollection<StockItem> _allItems = new();

		[ObservableProperty]
		private ObservableCollection<StockItem> items = new();

		[ObservableProperty]
		private StockItem? selectedItem;

		[ObservableProperty]
		private ObservableCollection<Category> categories = new();

		[ObservableProperty]
		private Category? selectedCategory;

		[ObservableProperty]
		private StockStatusFilter selectedStatusFilter = StockStatusFilter.All;

		[ObservableProperty]
		private int lowStockCount;

		[ObservableProperty]
		private int outOfStockCount;

		// ✅ Category Breakdown
		public ObservableCollection<CategoryValueBreakdown> CategoryBreakdown { get; }
			= new();

		// =========================
		// FINANCIAL SUMMARY
		// =========================

		public decimal TotalInventoryValue =>
			_allItems.Sum(i => i.TotalStockValue);

		public decimal TotalLowStockValue =>
			_allItems
				.Where(i => i.Status == StockStatus.Low)
				.Sum(i => i.TotalStockValue);

		public decimal TotalOutOfStockValue =>
			_allItems
				.Where(i => i.Status == StockStatus.Out)
				.Sum(i => i.TotalStockValue);

		public int TotalUnitsInStock =>
			_allItems.Sum(i => i.Quantity);

		public IEnumerable<StockItem> TopValuableItems =>
			_allItems
				.OrderByDescending(i => i.TotalStockValue)
				.Take(5);

		public bool HasStockWarnings =>
			LowStockCount > 0 || OutOfStockCount > 0;

		// =========================
		// COMMANDS
		// =========================

		public IRelayCommand ShowAllCommand { get; }
		public IRelayCommand ShowLowCommand { get; }
		public IRelayCommand ShowOutCommand { get; }

		public IRelayCommand NewItemCommand { get; }
		public IRelayCommand EditItemCommand { get; }
		public IRelayCommand StockInCommand { get; }
		public IRelayCommand StockOutCommand { get; }
		public IRelayCommand ViewHistoryCommand { get; }
		public IRelayCommand ExportWarningsCommand { get; }

		public StockViewModel()
		{
			_stockRepo = new StockRepository();
			_categoryRepo = new CategoryRepository();

			ShowAllCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.All));
			ShowLowCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.Low));
			ShowOutCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.Out));

			ExportWarningsCommand = new RelayCommand(ExportWarningsToExcel, () => HasStockWarnings);

			LoadCategories();
			RestoreLastStatusFilter();
			LoadItems();

			NewItemCommand = new RelayCommand(OpenNewItem);
			EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
			StockInCommand = new RelayCommand(OpenStockIn, () => SelectedItem != null);
			StockOutCommand = new RelayCommand(OpenStockOut, () => SelectedItem != null);
			ViewHistoryCommand = new RelayCommand(OpenHistory);
		}

		// =========================
		// LOAD DATA
		// =========================

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
			var all = _stockRepo.GetAll();

			_allItems = new ObservableCollection<StockItem>(
				(SelectedCategory == null || SelectedCategory.Name == "All")
					? all
					: all.Where(i => i.Category == SelectedCategory.Name)
			);

			ApplyFilters();

			if (Items.Count == 0 && SelectedStatusFilter != StockStatusFilter.All)
			{
				SelectedStatusFilter = StockStatusFilter.All;
				ApplyFilters();
			}

			RecalculateStatusCounters();
			BuildCategoryBreakdown();

			ExportWarningsCommand.NotifyCanExecuteChanged();

			OnPropertyChanged(nameof(TotalInventoryValue));
			OnPropertyChanged(nameof(TotalLowStockValue));
			OnPropertyChanged(nameof(TotalOutOfStockValue));
			OnPropertyChanged(nameof(TotalUnitsInStock));
			OnPropertyChanged(nameof(TopValuableItems));
		}

		// =========================
		// CATEGORY BREAKDOWN
		// =========================

		private void BuildCategoryBreakdown()
		{
			CategoryBreakdown.Clear();

			var grouped = _allItems
				.GroupBy(i => i.Category)
				.Select(g => new CategoryValueBreakdown
				{
					Category = g.Key,
					TotalValue = g.Sum(x => x.TotalStockValue),
					TotalUnits = g.Sum(x => x.Quantity)
				})
				.OrderByDescending(g => g.TotalValue);

			foreach (var item in grouped)
				CategoryBreakdown.Add(item);
		}

		// =========================
		// FILTERS
		// =========================

		private void ApplyStatusFilter(StockStatusFilter filter)
		{
			SelectedStatusFilter = filter;
			SaveLastStatusFilter();
			ApplyFilters();
		}

		private void ApplyFilters()
		{
			SelectedItem = null;

			Items = SelectedStatusFilter switch
			{
				StockStatusFilter.Low =>
					new ObservableCollection<StockItem>(_allItems.Where(i => i.Status == StockStatus.Low)),

				StockStatusFilter.Out =>
					new ObservableCollection<StockItem>(_allItems.Where(i => i.Status == StockStatus.Out)),

				_ => new ObservableCollection<StockItem>(_allItems)
			};
		}

		private void RecalculateStatusCounters()
		{
			OutOfStockCount = _allItems.Count(i => i.Status == StockStatus.Out);
			LowStockCount = _allItems.Count(i => i.Status == StockStatus.Low);

			OnPropertyChanged(nameof(HasStockWarnings));
		}

		private void SaveLastStatusFilter()
		{
			Application.Current.Properties[LastStatusFilterKey] = SelectedStatusFilter;
		}

		private void RestoreLastStatusFilter()
		{
			if (Application.Current.Properties[LastStatusFilterKey]
	is StockStatusFilter filter)
			{
				SelectedStatusFilter = filter;
			}
			else
			{
				SelectedStatusFilter = StockStatusFilter.All;
			}

		}

		// =========================
		// EXPORT
		// =========================

		private void ExportWarningsToExcel()
		{
			var warnings = _allItems
				.Where(i => i.Status == StockStatus.Low || i.Status == StockStatus.Out)
				.ToList();

			if (!warnings.Any())
				return;

			var dialog = new SaveFileDialog
			{
				Filter = "Excel Files (*.xlsx)|*.xlsx",
				FileName = $"StockWarnings_{DateTime.Today:yyyy-MM-dd}.xlsx"
			};

			if (dialog.ShowDialog() != true)
				return;

			using var workbook = new XLWorkbook();
			var ws = workbook.Worksheets.Add("Stock Warnings");

			ws.Cell(1, 1).Value = "Item Code";
			ws.Cell(1, 2).Value = "Description";
			ws.Cell(1, 3).Value = "Quantity";
			ws.Cell(1, 4).Value = "Unit";
			ws.Cell(1, 5).Value = "Category";
			ws.Cell(1, 6).Value = "Status";

			ws.Range(1, 1, 1, 6).Style.Font.Bold = true;

			int row = 2;
			foreach (var item in warnings)
			{
				ws.Cell(row, 1).Value = item.ItemCode;
				ws.Cell(row, 2).Value = item.Description;
				ws.Cell(row, 3).Value = item.Quantity;
				ws.Cell(row, 4).Value = item.Unit;
				ws.Cell(row, 5).Value = item.Category;
				ws.Cell(row, 6).Value = item.Status.ToString();
				row++;
			}

			ws.Columns().AdjustToContents();
			workbook.SaveAs(dialog.FileName);
		}

		// =========================
		// WINDOWS
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

		public void RefreshAfterCategoryChange()
		{
			LoadCategories();
			LoadItems();
		}
	}
}

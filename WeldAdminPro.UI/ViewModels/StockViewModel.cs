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
		ReorderRequired,
		Critical
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

		public ObservableCollection<CategoryValueBreakdown> CategoryBreakdown { get; }
			= new();

		// =========================
		// FINANCIAL SUMMARY
		// =========================

		public decimal TotalInventoryValue =>
			_allItems.Sum(i => i.TotalStockValue);

		public decimal TotalLowStockValue =>
			_allItems.Where(i => i.SmartStatus == SmartStockStatus.ReorderRequired)
					 .Sum(i => i.TotalStockValue);

		public decimal TotalOutOfStockValue =>
			_allItems.Where(i => i.SmartStatus == SmartStockStatus.Critical)
					 .Sum(i => i.TotalStockValue);

		public int TotalUnitsInStock =>
	(int)Math.Floor(_allItems.Sum(i => i.Quantity));

		public IEnumerable<StockItem> TopValuableItems =>
			_allItems.OrderByDescending(i => i.TotalStockValue)
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
		public IRelayCommand ViewLedgerCommand { get; }

		public IRelayCommand ExportWarningsCommand { get; }
		public IRelayCommand OpenSmartPlannerCommand { get; }


		public StockViewModel()
		{
			_stockRepo = new StockRepository();
			_categoryRepo = new CategoryRepository();

			ShowAllCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.All));
			ShowLowCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.ReorderRequired));
			ShowOutCommand = new RelayCommand(() => ApplyStatusFilter(StockStatusFilter.Critical));

			ExportWarningsCommand = new RelayCommand(ExportWarningsToExcel, () => HasStockWarnings);

			NewItemCommand = new RelayCommand(OpenNewItem);
			EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
			StockInCommand = new RelayCommand(OpenStockIn, () => SelectedItem != null);
			StockOutCommand = new RelayCommand(OpenStockOut, () => SelectedItem != null);

			ViewHistoryCommand = new RelayCommand(OpenHistory);
			ViewLedgerCommand = new RelayCommand(OpenLedger);
			OpenSmartPlannerCommand = new RelayCommand(OpenSmartPlanner);


			LoadCategories();
			RestoreLastStatusFilter();
			LoadItems();
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
					TotalUnits = (int)Math.Floor(g.Sum(x => x.Quantity))
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
				StockStatusFilter.ReorderRequired =>
					new ObservableCollection<StockItem>(
						_allItems.Where(i =>
							i.SmartStatus == SmartStockStatus.ReorderRequired)),

				StockStatusFilter.Critical =>
					new ObservableCollection<StockItem>(
						_allItems.Where(i =>
							i.SmartStatus == SmartStockStatus.Critical)),

				_ => new ObservableCollection<StockItem>(_allItems)
			};
		}

		private void RecalculateStatusCounters()
		{
			OutOfStockCount = _allItems.Count(i =>
				i.SmartStatus == SmartStockStatus.Critical);

			LowStockCount = _allItems.Count(i =>
				i.SmartStatus == SmartStockStatus.ReorderRequired);

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
		// EXPORT WARNINGS
		// =========================

		private void ExportWarningsToExcel()
		{
			var warnings = _allItems
				.Where(i =>
					i.SmartStatus == SmartStockStatus.Critical ||
					i.SmartStatus == SmartStockStatus.ReorderRequired)
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
			ws.Cell(1, 6).Value = "Smart Status";
			ws.Cell(1, 7).Value = "Suggested Reorder Qty";

			ws.Range(1, 1, 1, 7).Style.Font.Bold = true;

			int row = 2;
			foreach (var item in warnings)
			{
				ws.Cell(row, 1).Value = item.ItemCode;
				ws.Cell(row, 2).Value = item.Description;
				ws.Cell(row, 3).Value = item.Quantity;
				ws.Cell(row, 4).Value = item.Unit;
				ws.Cell(row, 5).Value = item.Category;
				ws.Cell(row, 6).Value = item.SmartStatus.ToString();
				ws.Cell(row, 7).Value = item.SuggestedReorderQuantity;
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
				Title = "New Stock Item",
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			if (Application.Current.MainWindow != null &&
				Application.Current.MainWindow != window)
			{
				window.Owner = Application.Current.MainWindow;
			}

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
				Title = "Edit Stock Item",
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			if (Application.Current.MainWindow != null &&
				Application.Current.MainWindow != window)
			{
				window.Owner = Application.Current.MainWindow;
			}

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
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			if (Application.Current.MainWindow != null &&
				Application.Current.MainWindow != window)
			{
				window.Owner = Application.Current.MainWindow;
			}

			vm.TransactionCompleted += LoadItems;
			vm.RequestClose += window.Close;

			window.ShowDialog();
		}


		private void OpenHistory()
		{
			var historyView = new StockTransactionHistoryView();

			var window = new Window
			{
				Title = "Stock Transaction History",
				Content = historyView,
				Width = 900,
				Height = 600,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			// SAFETY CHECK – prevent self-owner crash
			if (Application.Current.MainWindow != null &&
				Application.Current.MainWindow != window)
			{
				window.Owner = Application.Current.MainWindow;
			}

			window.ShowDialog();
		}


		private void OpenLedger()
		{
			var ledgerView = new StockLedgerView();

			var window = new Window
			{
				Title = "Stock Ledger",
				Content = ledgerView,
				Width = 1000,
				Height = 700
			};

			// Only assign owner if this is NOT already the main window
			if (Application.Current.MainWindow != null &&
				Application.Current.MainWindow != window)
			{
				window.Owner = Application.Current.MainWindow;
			}

			window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

			window.ShowDialog();
		}
		private void OpenSmartPlanner()
		{
			var window = new Window
			{
				Title = "Smart Reorder Planner",
				Content = new SmartReorderPlannerView(),
				Width = 1000,
				Height = 650
			};

			var main = Application.Current.MainWindow;
			if (main != null && main != window)
				window.Owner = main;

			window.ShowDialog();
		}



		public void RefreshAfterCategoryChange()
		{
			LoadCategories();
			LoadItems();
		}
	}
}

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockViewModel : ObservableObject
	{
		private readonly StockRepository _repo;
		private readonly CategoryRepository _categoryRepo;

		// =========================
		// Observable properties
		// =========================

		[ObservableProperty]
		private ObservableCollection<StockItem> items = new();

		[ObservableProperty]
		private StockItem? selectedItem;

		// =========================
		// Categories (dynamic)
		// =========================

		[ObservableProperty]
		private ObservableCollection<string> categories = new();

		[ObservableProperty]
		private string selectedCategory = "All";

		// =========================
		// Undo delete/edit
		// =========================

		private StockItem? _lastDeletedItem;
		private StockItem? _lastEditedBefore;

		[ObservableProperty]
		private bool canUndoDelete;

		[ObservableProperty]
		private bool canUndoEdit;

		// =========================
		// Commands
		// =========================

		public IRelayCommand NewItemCommand { get; }
		public IRelayCommand EditItemCommand { get; }
		public IRelayCommand StockInCommand { get; }
		public IRelayCommand StockOutCommand { get; }
		public IRelayCommand DeleteStockItemCommand { get; }
		public IRelayCommand UndoDeleteCommand { get; }
		public IRelayCommand UndoEditCommand { get; }

		// =========================
		// Constructor
		// =========================

		public StockViewModel()
		{
			_repo = new StockRepository();
			_categoryRepo = new CategoryRepository();

			LoadCategories();
			Reload();

			NewItemCommand = new RelayCommand(OpenNewItem);
			EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
			StockInCommand = new RelayCommand(OpenStockIn, () => SelectedItem != null);
			StockOutCommand = new RelayCommand(OpenStockOut, () => SelectedItem != null);
			DeleteStockItemCommand = new RelayCommand(DeleteSelectedStockItem, () => CanDeleteSelectedItem);
			UndoDeleteCommand = new RelayCommand(UndoLastDelete, () => CanUndoDelete);
			UndoEditCommand = new RelayCommand(UndoLastEdit, () => CanUndoEdit);
		}
		private void OnCategoriesChanged()
		{
			LoadCategories();
			Reload();
		}


		// =========================
		// Category loading
		// =========================

		public void LoadCategories()
		{
			Categories.Clear();
			Categories.Add("All");

			foreach (var cat in _categoryRepo.GetAllActive())
			{
				Categories.Add(cat.Name);
			}

			if (!Categories.Contains(SelectedCategory))
				SelectedCategory = "All";
		}

		partial void OnSelectedCategoryChanged(string value)
		{
			Reload();
		}

		// =========================
		// Reload stock items
		// =========================

		public void Reload()
		{
			SelectedItem = null;

			var allItems = _repo.GetAll();

			if (SelectedCategory != "All")
			{
				allItems = allItems
					.Where(i => i.Category == SelectedCategory)
					.ToList();
			}

			Items = new ObservableCollection<StockItem>(allItems);
		}

		partial void OnSelectedItemChanged(StockItem? value)
		{
			EditItemCommand.NotifyCanExecuteChanged();
			StockInCommand.NotifyCanExecuteChanged();
			StockOutCommand.NotifyCanExecuteChanged();
			DeleteStockItemCommand.NotifyCanExecuteChanged();
		}


		// =========================
		// New / Edit stock
		// =========================

		private void OpenNewItem()
		{
			var vm = new NewStockItemViewModel();

			var window = new NewStockItemWindow(vm)
			{
				Owner = Application.Current.MainWindow,
				Title = "New Stock Item"
			};

			vm.ItemCreated += () =>
			{
				LoadCategories();

				// 🔑 CRITICAL FIX:
				SelectedCategory = "All";

				Reload();
			};


			vm.RequestClose += window.Close;
			window.ShowDialog();
		}

		private void OpenEditItem()
		{
			if (SelectedItem == null)
				return;

			_lastEditedBefore = new StockItem
			{
				Id = SelectedItem.Id,
				ItemCode = SelectedItem.ItemCode,
				Description = SelectedItem.Description,
				Quantity = SelectedItem.Quantity,
				Unit = SelectedItem.Unit,
				Category = SelectedItem.Category
			};

			var vm = new NewStockItemViewModel(SelectedItem);

			var window = new NewStockItemWindow(vm)
			{
				Owner = Application.Current.MainWindow,
				Title = "Edit Stock Item"
			};

			vm.ItemCreated += () =>
			{
				CanUndoEdit = true;
				LoadCategories();
				Reload();
			};

			vm.RequestClose += window.Close;
			window.ShowDialog();
		}

		// =========================
		// Transactions
		// =========================

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

			vm.TransactionCompleted += Reload;
			vm.RequestClose += window.Close;
			window.ShowDialog();
		}

		// =========================
		// Delete / Undo
		// =========================

		private void DeleteSelectedStockItem()
		{
			if (SelectedItem == null)
				return;

			if (MessageBox.Show(
				$"Delete '{SelectedItem.ItemCode}'?",
				"Confirm Delete",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning) != MessageBoxResult.Yes)
				return;

			_lastDeletedItem = SelectedItem;
			CanUndoDelete = true;

			_repo.DeleteStockItem(SelectedItem.Id);
			Reload();
		}

		private void UndoLastDelete()
		{
			if (_lastDeletedItem == null)
				return;

			_repo.Add(_lastDeletedItem);
			_lastDeletedItem = null;
			CanUndoDelete = false;

			Reload();
		}

		private void UndoLastEdit()
		{
			if (_lastEditedBefore == null)
				return;

			_repo.Update(_lastEditedBefore);
			_lastEditedBefore = null;
			CanUndoEdit = false;

			Reload();
		}
		public bool CanDeleteSelectedItem =>
		SelectedItem != null && !_repo.HasTransactions(SelectedItem.Id);

	}
}
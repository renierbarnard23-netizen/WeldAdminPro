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

		// =========================
		// Observable properties
		// =========================

		[ObservableProperty]
		private ObservableCollection<StockItem> items = new();

		[ObservableProperty]
		private StockItem? selectedItem;

		// =========================
		// Category filter (manual)
		// =========================

		public ObservableCollection<string> Categories { get; } =
			new()
			{
				"All",
				"Uncategorised",
				"Electrodes",
				"Gas",
				"Abrasives",
				"PPE"
			};

		private string selectedCategory = "All";
		public string SelectedCategory
		{
			get => selectedCategory;
			set
			{
				if (SetProperty(ref selectedCategory, value))
				{
					Reload();
				}
			}
		}

		// =========================
		// Undo delete (session-only)
		// =========================

		private StockItem? _lastDeletedItem;

		[ObservableProperty]
		private bool canUndoDelete;

		// =========================
		// Undo edit (session-only)
		// =========================

		private StockItem? _lastEditedBefore;

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

			Reload();

			NewItemCommand = new RelayCommand(OpenNewItem);
			EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
			StockInCommand = new RelayCommand(OpenStockIn, () => SelectedItem != null);
			StockOutCommand = new RelayCommand(OpenStockOut, () => SelectedItem != null);
			DeleteStockItemCommand = new RelayCommand(DeleteSelectedStockItem, () => SelectedItem != null);
			UndoDeleteCommand = new RelayCommand(UndoLastDelete, () => CanUndoDelete);
			UndoEditCommand = new RelayCommand(UndoLastEdit, () => CanUndoEdit);
		}

		// =========================
		// Reload & filtering (FIXED)
		// =========================

		public void Reload()
		{
			SelectedItem = null;

			var allItems = _repo.GetAll();

			// Normalise empty category
			foreach (var item in allItems)
			{
				if (string.IsNullOrWhiteSpace(item.Category))
					item.Category = "Uncategorised";
			}

			if (SelectedCategory != "All")
			{
				allItems = allItems
					.Where(i =>
						string.Equals(i.Category, SelectedCategory,
									  StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			Items = new ObservableCollection<StockItem>(allItems);
		}

		// =========================
		// Selection change handler
		// =========================

		partial void OnSelectedItemChanged(StockItem? value)
		{
			EditItemCommand.NotifyCanExecuteChanged();
			StockInCommand.NotifyCanExecuteChanged();
			StockOutCommand.NotifyCanExecuteChanged();
			DeleteStockItemCommand.NotifyCanExecuteChanged();
		}

		partial void OnCanUndoDeleteChanged(bool value)
		{
			UndoDeleteCommand.NotifyCanExecuteChanged();
		}

		partial void OnCanUndoEditChanged(bool value)
		{
			UndoEditCommand.NotifyCanExecuteChanged();
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

			vm.ItemCreated += Reload;
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
				Reload();
			};

			vm.RequestClose += window.Close;

			window.ShowDialog();
		}

		// =========================
		// Stock transactions
		// =========================

		private void OpenStockIn() => OpenTransaction(true);
		private void OpenStockOut() => OpenTransaction(false);

		private void OpenTransaction(bool isStockIn)
		{
			ClearEditUndo();

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
		// Delete stock item
		// =========================

		private void DeleteSelectedStockItem()
		{
			if (SelectedItem == null)
				return;

			if (MessageBox.Show(
					$"Delete stock item '{SelectedItem.ItemCode}'?",
					"Confirm Delete",
					MessageBoxButton.YesNo,
					MessageBoxImage.Warning) != MessageBoxResult.Yes)
				return;

			try
			{
				ClearEditUndo();

				_lastDeletedItem = SelectedItem;
				CanUndoDelete = true;

				_repo.DeleteStockItem(SelectedItem.Id);
				Reload();
			}
			catch (InvalidOperationException ex)
			{
				MessageBox.Show(ex.Message, "Delete blocked",
					MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		// =========================
		// Undo delete
		// =========================

		private void UndoLastDelete()
		{
			if (_lastDeletedItem == null)
				return;

			_repo.Add(_lastDeletedItem);

			_lastDeletedItem = null;
			CanUndoDelete = false;

			Reload();
		}

		// =========================
		// Undo edit
		// =========================

		private void UndoLastEdit()
		{
			if (_lastEditedBefore == null)
				return;

			_repo.Update(_lastEditedBefore);

			_lastEditedBefore = null;
			CanUndoEdit = false;

			Reload();
		}

		private void ClearEditUndo()
		{
			_lastEditedBefore = null;
			CanUndoEdit = false;
		}
	}
}

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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
        // Commands
        // =========================

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand EditItemCommand { get; }
        public IRelayCommand StockInCommand { get; }
        public IRelayCommand StockOutCommand { get; }
        public IRelayCommand DeleteStockItemCommand { get; }

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
            DeleteStockItemCommand = new RelayCommand(DeleteSelectedStockItem, CanDeleteStockItem);
        }

        // =========================
        // SINGLE SOURCE OF TRUTH
        // =========================

        public void Reload()
        {
            SelectedItem = null; // force DataGrid refresh
            Items = new ObservableCollection<StockItem>(_repo.GetAll());
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

            var vm = new NewStockItemViewModel(SelectedItem);

            var window = new NewStockItemWindow(vm)
            {
                Owner = Application.Current.MainWindow,
                Title = "Edit Stock Item"
            };

            vm.ItemCreated += Reload;
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
            if (SelectedItem == null)
                return;

            var vm = new StockTransactionViewModel(SelectedItem, isStockIn);

            var window = new StockTransactionWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };

            // 🔑 CRITICAL FIX
            vm.TransactionCompleted += Reload;
            vm.RequestClose += window.Close;

            window.ShowDialog();
        }

        // =========================
        // Delete stock item (SAFE)
        // =========================

        private bool CanDeleteStockItem()
        {
            return SelectedItem != null;
        }

        private void DeleteSelectedStockItem()
        {
            if (SelectedItem == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete stock item '{SelectedItem.ItemCode}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repo.DeleteStockItem(SelectedItem.Id);
                Reload();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Delete Stock Item",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}

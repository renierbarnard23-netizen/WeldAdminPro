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

        [ObservableProperty]
        private ObservableCollection<StockItem> items = new();

        [ObservableProperty]
        private StockItem? selectedItem;

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand EditItemCommand { get; }

        public StockViewModel()
        {
            _repo = new StockRepository();
            LoadItems();

            NewItemCommand = new RelayCommand(OpenNewItem);
            EditItemCommand = new RelayCommand(OpenEditItem, () => SelectedItem != null);
        }

        // 🔑 THIS IS WHAT ENABLES THE EDIT BUTTON
        partial void OnSelectedItemChanged(StockItem? value)
        {
            EditItemCommand.NotifyCanExecuteChanged();
        }

        private void LoadItems()
{
    Items = new ObservableCollection<StockItem>(_repo.GetAll());
    SelectedItem = null;
}


        private void OpenNewItem()
        {
            var vm = new NewStockItemViewModel();

            var window = new NewStockItemWindow(vm)
            {
                Owner = Application.Current.MainWindow,
                Title = "New Stock Item"
            };

            vm.ItemCreated += LoadItems;
            vm.RequestClose += () => window.Close();

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

            vm.ItemCreated += LoadItems;
            vm.RequestClose += () => window.Close();

            window.ShowDialog();
        }
    }
}

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

        public IRelayCommand NewItemCommand { get; }

        public StockViewModel()
        {
            _repo = new StockRepository();
            LoadItems();

            NewItemCommand = new RelayCommand(OpenNewItem);
        }

        private void LoadItems()
        {
            Items.Clear();
            foreach (var item in _repo.GetAll())
                Items.Add(item);
        }

        private void OpenNewItem()
        {
            var vm = new NewStockItemViewModel();

            var window = new NewStockItemWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };

            // Reload list after save
            vm.ItemCreated += LoadItems;

            // Close dialog
            vm.RequestClose += () => window.Close();

            window.ShowDialog();
        }
    }
}

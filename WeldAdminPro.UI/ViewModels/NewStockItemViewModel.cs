using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class NewStockItemViewModel : ObservableObject
    {
        private readonly StockRepository _repo = new();

        [ObservableProperty]
        private StockItem item = new();

        public event Action? ItemCreated;
        public event Action? RequestClose;

        public NewStockItemViewModel()
        {
            Item.Id = Guid.NewGuid();
        }

        [RelayCommand]
        private void Save()
        {
            // 🔐 Persist to database
            _repo.Add(Item);

            // Notify parent view
            ItemCreated?.Invoke();

            // Close window
            RequestClose?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
}

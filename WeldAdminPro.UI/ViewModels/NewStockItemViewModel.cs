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
        private StockItem item;

        public bool IsEditMode { get; }

        public event Action? ItemCreated;
        public event Action? RequestClose;

        // NEW item constructor
        public NewStockItemViewModel()
        {
            Item = new StockItem
            {
                Id = Guid.NewGuid(),
                Quantity = 0
            };
            IsEditMode = false;
        }

        // EDIT item constructor (THIS FIXES YOUR ERROR)
        public NewStockItemViewModel(StockItem existingItem)
        {
            Item = new StockItem
            {
                Id = existingItem.Id,
                ItemCode = existingItem.ItemCode,
                Description = existingItem.Description,
                Quantity = existingItem.Quantity,
                Unit = existingItem.Unit
            };
            IsEditMode = true;
        }

        [RelayCommand]
        private void Save()
        {
            if (IsEditMode)
                _repo.Update(Item);
            else
                _repo.Add(Item);

            ItemCreated?.Invoke();
            RequestClose?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
}

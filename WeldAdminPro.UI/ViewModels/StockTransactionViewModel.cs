using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class StockTransactionViewModel : ObservableObject
    {
        private readonly StockRepository _repo;
        private readonly bool _isStockIn;

        public StockItem Item { get; }

        // 🔑 STRING binding avoids WPF int binding failures
        [ObservableProperty]
        private string quantityText = string.Empty;

        [ObservableProperty]
        private string reference = string.Empty;

        public string Title => _isStockIn ? "Stock IN" : "Stock OUT";

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public event Action? TransactionCompleted;
        public event Action? RequestClose;

        public StockTransactionViewModel(StockItem item, bool isStockIn)
        {
            Item = item;
            _isStockIn = isStockIn;
            _repo = new StockRepository();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Save()
        {
            if (!int.TryParse(QuantityText, out int qty) || qty <= 0)
            {
                MessageBox.Show("Quantity must be greater than zero.");
                return;
            }

            // 🔒 Stock OUT validation
            if (!_isStockIn)
            {
                if (Item.Quantity < qty)
                {
                    MessageBox.Show("Insufficient stock for Stock OUT.");
                    return;
                }

                qty = -qty;
            }

            var tx = new StockTransaction
            {
                Id = Guid.NewGuid(),
                StockItemId = Item.Id,
                TransactionDate = DateTime.Now,
                Quantity = qty,
                Type = _isStockIn ? "IN" : "OUT",
                Reference = Reference
            };

            _repo.AddTransaction(tx);

            TransactionCompleted?.Invoke();
            RequestClose?.Invoke();
        }
    }
}

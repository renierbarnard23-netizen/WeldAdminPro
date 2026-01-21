using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public class StockTransactionHistoryViewModel
    {
        private readonly StockRepository _repository;

        public ObservableCollection<StockTransaction> Transactions { get; }

        public IRelayCommand RefreshCommand { get; }

        public StockTransactionHistoryViewModel()
        {
            _repository = new StockRepository();
            Transactions = new ObservableCollection<StockTransaction>();

            RefreshCommand = new RelayCommand(Reload);

            Reload();
        }

        public void Reload()
        {
            Transactions.Clear();

            // Get all transactions (oldest → newest)
            var transactions = _repository.GetAllTransactions()
                                           .OrderBy(t => t.TransactionDate)
                                           .ToList();

            // Get current stock quantities (SOURCE OF TRUTH)
            var stockItems = _repository.GetAll()
                                        .ToDictionary(i => i.Id, i => i.Quantity);

            // Running balance tracker (start from CURRENT quantity)
            var runningBalances = new Dictionary<Guid, int>(stockItems);

            // Walk transactions BACKWARDS so latest balance matches stock item quantity
            foreach (var tx in transactions.OrderByDescending(t => t.TransactionDate))
            {
                if (!runningBalances.ContainsKey(tx.StockItemId))
                    runningBalances[tx.StockItemId] = 0;

                // Balance AFTER this transaction
                tx.RunningBalance = runningBalances[tx.StockItemId];

                // Reverse this transaction to get previous balance
                runningBalances[tx.StockItemId] -=
                    tx.Type == "IN" ? tx.Quantity : -tx.Quantity;
            }

            // Display newest first
            foreach (var tx in transactions.OrderByDescending(t => t.TransactionDate))
                Transactions.Add(tx);
        }
    }
}

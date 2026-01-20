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

            // Get current stock balances
            var stockItems = _repository.GetAll()
                                        .ToDictionary(i => i.Id, i => i.Quantity);

            // Calculate opening balances
            var openingBalances = new Dictionary<Guid, int>();

            foreach (var item in stockItems)
            {
                int totalMovement = transactions
                    .Where(t => t.StockItemId == item.Key)
                    .Sum(t => t.Type == "IN" ? t.Quantity : -t.Quantity);

                openingBalances[item.Key] = item.Value - totalMovement;
            }

            // Running balance tracker
            var runningBalances = new Dictionary<Guid, int>(openingBalances);

            // Replay transactions forward
            foreach (var tx in transactions)
            {
                if (!runningBalances.ContainsKey(tx.StockItemId))
                    runningBalances[tx.StockItemId] = 0;

                runningBalances[tx.StockItemId] +=
                    tx.Type == "IN" ? tx.Quantity : -tx.Quantity;

                tx.RunningBalance = runningBalances[tx.StockItemId];
            }

            // Display newest first
            foreach (var tx in transactions.OrderByDescending(t => t.TransactionDate))
                Transactions.Add(tx);
        }
    }
}

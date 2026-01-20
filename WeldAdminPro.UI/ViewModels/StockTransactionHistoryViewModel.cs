using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using System.Linq;

namespace WeldAdminPro.UI.ViewModels
{
    public class StockTransactionHistoryViewModel

    {
        private readonly StockRepository _repository;

        public ObservableCollection<StockTransaction> Transactions { get; }

        public StockTransactionHistoryViewModel()
        {
            _repository = new StockRepository();
            Transactions = new ObservableCollection<StockTransaction>();

            Load();
        }

        private void Load()
{
    Transactions.Clear();

    var all = _repository.GetAllTransactions();

    // Group by Stock Item
    foreach (var group in all
        .GroupBy(t => t.StockItemId))
    {
        int balance = 0;

        // IMPORTANT:
        // Calculate in chronological order
        foreach (var tx in group
            .OrderBy(t => t.TransactionDate))
        {
            balance += tx.Quantity;
            tx.RunningBalance = balance;
        }
    }

    // Display newest first (UI only)
    foreach (var tx in all
        .OrderByDescending(t => t.TransactionDate))
    {
        Transactions.Add(tx);
    }
}

    }
}

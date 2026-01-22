using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ConsumableTransactionHistoryViewModel : ObservableObject
	{
		private readonly StockTransactionRepository _transactionRepo;
		private readonly StockItem _stockItem;

		[ObservableProperty]
		private ObservableCollection<StockTransaction> transactions = new();

		public ConsumableTransactionHistoryViewModel(StockItem stockItem)
		{
			_stockItem = stockItem;
			_transactionRepo = new StockTransactionRepository();

			LoadTransactions();
		}

		private void LoadTransactions()
		{
			Transactions.Clear();

			var txs = _transactionRepo
				.GetByStockItem(_stockItem.Id)
				.OrderBy(t => t.TransactionDate)
				.ToList();

			int runningBalance = 0;

			foreach (var tx in txs)
			{
				runningBalance += tx.SignedQuantity;
				tx.RunningBalance = runningBalance;

				Transactions.Add(tx);
			}
		}
	}
}

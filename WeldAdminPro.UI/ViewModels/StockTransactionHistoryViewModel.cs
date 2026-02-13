using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionHistoryViewModel : ObservableObject
	{
		private readonly StockRepository _repo;

		[ObservableProperty]
		private ObservableCollection<StockTransaction> transactions = new();

		public StockTransactionHistoryViewModel()
		{
			_repo = new StockRepository();
			LoadTransactions();
		}

		private void LoadTransactions()
		{
			var list = _repo.GetAllTransactions();
			Transactions = new ObservableCollection<StockTransaction>(list);
		}
	}
}

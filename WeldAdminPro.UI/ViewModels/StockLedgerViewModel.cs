	using CommunityToolkit.Mvvm.ComponentModel;
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using WeldAdminPro.Core.Models;
	using WeldAdminPro.Data.Repositories;

	namespace WeldAdminPro.UI.ViewModels
	{
		public class StockLedgerGroup
		{
			public string ItemCode { get; set; } = string.Empty;
			public string Description { get; set; } = string.Empty;

			public ObservableCollection<StockTransaction> Transactions { get; set; }
				= new();

			public int CurrentBalance =>
				Transactions.LastOrDefault()?.BalanceAfter ?? 0;
		}

		public partial class StockLedgerViewModel : ObservableObject
		{
			private readonly StockRepository _repository;

			private ObservableCollection<StockLedgerGroup> _ledgerGroups = new();
			public ObservableCollection<StockLedgerGroup> LedgerGroups
			{
				get => _ledgerGroups;
				set => SetProperty(ref _ledgerGroups, value);
			}

			public StockLedgerViewModel()
			{
				_repository = new StockRepository();
				LoadLedger();
			}

			private void LoadLedger()
			{
				var transactions = _repository
					.GetAllTransactions()
					.OrderBy(t => t.TransactionDate)
					.ToList();

				var grouped = transactions
					.GroupBy(t => new { t.StockItemId, t.ItemCode, t.ItemDescription })
					.OrderBy(g => g.Key.ItemCode);

				var result = new ObservableCollection<StockLedgerGroup>();

				foreach (var group in grouped)
				{
					result.Add(new StockLedgerGroup
					{
						ItemCode = group.Key.ItemCode,
						Description = group.Key.ItemDescription,
						Transactions = new ObservableCollection<StockTransaction>(group)
					});
				}

				LedgerGroups = result;
			}
		}
	}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data;


namespace WeldAdminPro.UI.ViewModels
{
	public partial class FixDuplicateItemCodesViewModel : ObservableObject
	{
		private readonly StockRepository _repo = new();

		[ObservableProperty]
		private ObservableCollection<DuplicateStockItem> duplicates = new();

		public event Action? FixCompleted;
		public event Action? RequestClose;

		public FixDuplicateItemCodesViewModel()
		{
			Load();
		}

		private void Load()
		{
			var raw = _repo.GetDuplicateItemCodes()
				.GroupBy(x => x.ItemCode, StringComparer.OrdinalIgnoreCase);

			foreach (var group in raw)
			{
				bool first = true;
				foreach (var item in group)
				{
					if (first)
					{
						item.ProposedItemCode = item.ItemCode; // keep
						first = false;
					}
					else
					{
						item.ProposedItemCode = _repo.GetNextAvailableItemCode(group.Key);
					}

					Duplicates.Add(item);
				}
			}
		}

		[RelayCommand]
		private void ApplyFixes()
		{
			using var connection = new SqliteConnection($"Data Source={DatabasePath.Get()}");
			connection.Open();

			using var tx = connection.BeginTransaction();

			foreach (var item in Duplicates.Where(d => d.ItemCode != d.ProposedItemCode))
			{
				_repo.RenameItemCode(item.Id, item.ProposedItemCode, connection);
			}

			tx.Commit();

			FixCompleted?.Invoke();
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}

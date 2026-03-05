using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class AuditViewModel : ObservableObject
	{
		private readonly StockRepository _repository;

		public ObservableCollection<StockTransaction> AuditEntries { get; }
			= new ObservableCollection<StockTransaction>();

		public IRelayCommand LoadCommand { get; }

		public AuditViewModel()
		{
			_repository = new StockRepository();
			LoadCommand = new RelayCommand(LoadAuditLog);
		}

		private void LoadAuditLog()
		{
			AuditEntries.Clear();

			var entries = _repository.GetAuditLog();

			foreach (var entry in entries)
			{
				AuditEntries.Add(entry);
			}
		}
	}
}
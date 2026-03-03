using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class AuditViewModel : ObservableObject
	{
		private readonly StockRepository _repository = new();

		[ObservableProperty]
		private ObservableCollection<AuditEntry> auditEntries = new();

		[ObservableProperty]
		private DateTime? fromDate;

		[ObservableProperty]
		private DateTime? toDate;

		[ObservableProperty]
		private string selectedSeverity = string.Empty;

		public IRelayCommand LoadCommand { get; }

		public AuditViewModel()
		{
			LoadCommand = new RelayCommand(LoadAudit);
			LoadAudit();
		}

		private void LoadAudit()
		{
			var data = _repository.GetAuditLog(
				FromDate,
				ToDate,
				string.IsNullOrWhiteSpace(SelectedSeverity)
					? null
					: SelectedSeverity);

			AuditEntries = new ObservableCollection<AuditEntry>(data);
		}
	}
}
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class AuditLogViewModel
        : ObservableObject
    {
        private readonly AuditLogRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<AuditLog>
            logs = new();

        public AuditLogViewModel()
        {
            _repository =
                new AuditLogRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            var logs =
                _repository.GetAll();

            Logs =
                new ObservableCollection<AuditLog>(
                    logs);
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class RepairManagementViewModel
        : ObservableObject
    {
        private readonly RepairRepository
            _repository;

        private readonly RepairAnalyticsService
            _analyticsService = new();

        [ObservableProperty]
        private ObservableCollection<RepairRecord>
            repairs = new();

        [ObservableProperty]
        private RepairRecord? selectedRepair;

        [ObservableProperty]
        private int totalRepairs;

        [ObservableProperty]
        private int openRepairs;

        [ObservableProperty]
        private RepairAnalytics analytics
            = new();

        public RepairManagementViewModel()
        {
            _repository =
                new RepairRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Repairs.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Repairs.Add(item);
            }

            TotalRepairs =
                Repairs.Count;

            OpenRepairs =
                Repairs.Count(x =>
                    x.Status != RepairStatus.Closed);

            Analytics =
                _analyticsService.Generate(
                    Repairs.ToList());
        }

        [RelayCommand]
        private void Refresh()
        {
            Load();
        }

        [RelayCommand]
        private void CloseRepair()
        {
            // =====================================
            // SECURITY
            // =====================================

            if (!PermissionService.HasPermission(
                SystemPermission.ApproveRepairs))
            {
                MessageBox.Show(
                    "You do not have permission to close repairs.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // =====================================
            // VALIDATION
            // =====================================

            if (SelectedRepair == null)
            {
                MessageBox.Show(
                    "Please select a repair.");

                return;
            }

            // =====================================
            // ALREADY CLOSED
            // =====================================

            if (SelectedRepair.Status ==
                RepairStatus.Closed)
            {
                MessageBox.Show(
                    "Repair is already closed.");

                return;
            }

            // =====================================
            // CLOSE REPAIR
            // =====================================

            SelectedRepair.Status =
                RepairStatus.Closed;

            SelectedRepair.CompletedDate =
                System.DateTime.UtcNow;
         

            // =====================================
            // SAVE
            // =====================================

            _repository.Update(
                SelectedRepair);

            // =====================================
            // AUDIT
            // =====================================

            AuditService.Log(
                "CLOSE REPAIR",
                "Quality",
                $"Closed repair: {SelectedRepair.RepairNumber}");

            // =====================================
            // REFRESH
            // =====================================

            Load();

            MessageBox.Show(
                "Repair closed successfully.");
        }
    }
}
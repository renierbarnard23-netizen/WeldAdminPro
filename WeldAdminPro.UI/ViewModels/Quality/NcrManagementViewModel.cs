using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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
    public partial class NcrManagementViewModel
        : ObservableObject
    {
        private readonly NcrRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<NcrRecord>
            ncrs = new();

        [ObservableProperty]
        private NcrRecord? selectedNcr;

        public NcrManagementViewModel()
        {
            _repository =
                new NcrRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Ncrs.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Ncrs.Add(item);
            }
        }

        [RelayCommand]
        private void CloseNcr()
        {
            if (SelectedNcr == null)
                return;

            if (!PermissionService.HasPermission(
                SystemPermission.CloseNcrs))
            {
                MessageBox.Show(
                    "You do not have permission to close NCRs.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!NcrWorkflowService.CanMoveTo(
                SelectedNcr.Status,
                NcrStatus.Closed))
            {
                MessageBox.Show(
                    "NCR cannot be closed until verification is complete.");

                return;
            }

            SelectedNcr.Status =
                NcrStatus.Closed;

            SelectedNcr.IsClosed =
                true;

            SelectedNcr.ClosedBy =
                CurrentUserContext.Username;

            SelectedNcr.ClosedDate =
                DateTime.UtcNow;

            _repository.Update(
                SelectedNcr);

            AuditService.Log(
                "CLOSE NCR",
                "Quality",
                $"Closed NCR: {SelectedNcr.NcrNumber}");

            Load();

            MessageBox.Show(
                "NCR closed successfully.");
        }

        [RelayCommand]
        private void StartInvestigation()
        {
            if (SelectedNcr == null)
                return;

            if (!NcrWorkflowService.CanMoveTo(
                SelectedNcr.Status,
                NcrStatus.UnderInvestigation))
            {
                MessageBox.Show(
                    "Invalid NCR workflow transition.");

                return;
            }

            SelectedNcr.Status =
                NcrStatus.UnderInvestigation;

            _repository.Update(
                SelectedNcr);

            AuditService.Log(
                "START NCR INVESTIGATION",
                "Quality",
                $"NCR: {SelectedNcr.NcrNumber}");

            Load();
        }

        [RelayCommand]
        private void AssignCorrectiveAction()
        {
            if (SelectedNcr == null)
                return;

            if (!PermissionService.HasPermission(
                SystemPermission.ApproveNcrDisposition))
            {
                MessageBox.Show(
                    "You do not have permission to approve NCR dispositions.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!NcrWorkflowService.CanMoveTo(
                SelectedNcr.Status,
                NcrStatus.AwaitingDisposition))
            {
                MessageBox.Show(
                    "Invalid NCR workflow transition.");

                return;
            }

            SelectedNcr.Status =
                NcrStatus.AwaitingDisposition;

            _repository.Update(
                SelectedNcr);

            AuditService.Log(
                "ASSIGN NCR DISPOSITION",
                "Quality",
                $"NCR: {SelectedNcr.NcrNumber}");

            Load();
        }

        [RelayCommand]
        private void Verify()
        {
            if (SelectedNcr == null)
                return;

            if (!PermissionService.HasPermission(
                SystemPermission.VerifyNcr))
            {
                MessageBox.Show(
                    "You do not have permission to verify NCRs.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!NcrWorkflowService.CanMoveTo(
                SelectedNcr.Status,
                NcrStatus.PendingVerification))
            {
                MessageBox.Show(
                    "Invalid NCR workflow transition.");

                return;
            }

            SelectedNcr.Status =
                NcrStatus.PendingVerification;

            _repository.Update(
                SelectedNcr);

            AuditService.Log(
                "VERIFY NCR",
                "Quality",
                $"NCR: {SelectedNcr.NcrNumber}");

            Load();
        }
    }
}
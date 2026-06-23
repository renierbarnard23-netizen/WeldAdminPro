using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class RepairRegisterViewModel
    : ObservableObject
    {
        private readonly RepairRepository
        _repairRepository;

        private readonly RepairWorkflowService
        _workflowService;

        private readonly IProjectContextService
        _projectContextService =
        App.ProjectContextService;


        private readonly IWeldService
            _weldService;

        private readonly WeldWorkflowEngine
            _weldWorkflowEngine;

        private readonly IHistoryTrackingService
            _historyTrackingService;
        
        [ObservableProperty]
        private ObservableCollection<RepairRecord>
            repairs = new();

        [ObservableProperty]
        private RepairRecord? selectedRepair;

        public RepairRegisterViewModel(
            IWeldService weldService)
        {
            _weldService =
                weldService;

            _projectContextService.ProjectChanged
                += OnProjectChanged;

            _repairRepository =
                new RepairRepository(
                    DatabasePath.GetConnectionString());

            _workflowService =
                new RepairWorkflowService();

            _weldWorkflowEngine =
                new WeldWorkflowEngine();

            _historyTrackingService =
                new HistoryTrackingService(
                DatabasePath.GetConnectionString());

            LoadCurrentProjectData();
        }

        private async void LoadCurrentProjectData()
        {
            Repairs.Clear();

            var project =
                _projectContextService.CurrentProject;

            if (project == null)
                return;

            var welds =
                await _weldService
                    .GetByProjectAsync(
                        project.Id);

            foreach (var weld in welds)
            {
                var repairs =
                    _repairRepository.GetByWeld(
                        weld.Id);

                foreach (var repair in repairs)
                {
                    Repairs.Add(repair);
                }
            }
        }

        private async Task<Weld?> GetRelatedWeldAsync(
Guid weldId)
        {
            var project =
            _projectContextService.CurrentProject;

if (project == null)
                return null;

            var welds =
                await _weldService.GetByProjectAsync(
                    project.Id);

            return welds.FirstOrDefault(
                x => x.Id == weldId);

}


        private void OnProjectChanged(
            Project? project)
        {
            LoadCurrentProjectData();
        }

        [RelayCommand]
        private void AuthorizeRepair()
        {
            if (SelectedRepair == null)
                return;

            if (!_workflowService.AuthorizeRepair(
                SelectedRepair,
                Environment.UserName,
                out var error))
            {
                MessageBox.Show(error);

                return;
            }

            _repairRepository.Update(
                SelectedRepair);

            MessageBox.Show(
                "Repair authorized.");
        }

        [RelayCommand]
        private void StartExcavation()
        {
            if (SelectedRepair == null)
                return;

            if (!_workflowService.StartExcavation(
                SelectedRepair,
                out var error))
            {
                MessageBox.Show(error);

                return;
            }

            _repairRepository.Update(
                SelectedRepair);

            MessageBox.Show(
                "Excavation started.");
        }


        [RelayCommand]
        private void StartRepairWelding()
        {
            if (SelectedRepair == null)
                return;

            _repairRepository.Update(
                SelectedRepair);


            if (!_workflowService.StartRepairWelding(
                    SelectedRepair,
                    out var error))
            {
                MessageBox.Show(error);

                return;
            }

            MessageBox.Show(
                "Repair welding started.");
        }

        [RelayCommand]
        private async Task SendForReinspection()
        {
            if (SelectedRepair == null)
                return;

if (!_workflowService.SendForReinspection(
        SelectedRepair,
        out var error))
            {
                MessageBox.Show(error);

                return;
            }

            _repairRepository.Update(
                SelectedRepair);

            var weld =
                await GetRelatedWeldAsync(
                    SelectedRepair.WeldId);

            if (weld != null)
            {
                _weldWorkflowEngine
                    .MarkReinspectionRequired(
                        weld,
                        out _);

                await _weldService.UpdateAsync(
                    weld);

                _historyTrackingService.Track(
                    weld,
                    "Repair Reinspection",
                    $"Repair #{SelectedRepair.RepairNumber} sent for reinspection.");
            }

            MessageBox.Show(
                "Repair sent for reinspection.");

}

        [RelayCommand]
        private async Task AcceptRepair()
        {
            if (SelectedRepair == null)
                return;

            if (!_workflowService.AcceptRepair(
                SelectedRepair,
                out var error))
            {
                MessageBox.Show(error);

                return;
            }

            _repairRepository.Update(
                SelectedRepair);

            _historyTrackingService.Track(
                new Weld
            {
                    Id = SelectedRepair.WeldId
            },
                "Repair Updated",
                $"Repair #{SelectedRepair.RepairNumber} " +
                $"status changed to " +
                $"{SelectedRepair.Status}.");


            var weld =
                await GetRelatedWeldAsync(
                    SelectedRepair.WeldId);

            if (weld != null)
            {
                _weldWorkflowEngine
                    .MarkAccepted(
                        weld,
                        out _);

                await _weldService.UpdateAsync(
                    weld);

                _historyTrackingService.Track(
                    weld,
                    "Repair Accepted",
                    $"Repair #{SelectedRepair.RepairNumber} accepted.");
            }

            MessageBox.Show(
                "Repair accepted.");
}


        [RelayCommand]
        private void CloseRepair()
        {
            if (SelectedRepair == null)
                return;

            _repairRepository.Update(
                SelectedRepair);

            _historyTrackingService.Track(
            new Weld
            {
                Id = SelectedRepair.WeldId
            },
            "Repair Closed",
            $"Repair #{SelectedRepair.RepairNumber} closed.");


            if (!_workflowService.CloseRepair(
                    SelectedRepair,
                    out var error))
            {
                MessageBox.Show(error);

                return;
            }

            MessageBox.Show(
                "Repair closed.");
        }
    }
}

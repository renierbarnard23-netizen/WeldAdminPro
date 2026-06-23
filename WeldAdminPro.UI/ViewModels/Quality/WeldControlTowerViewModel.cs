using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using WeldAdminPro.Core.Events;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class WeldControlTowerViewModel
        : ObservableObject
    {
        private readonly WeldRepository
            _weldRepository =
                new();

        private readonly WeldControlTowerService
            _service =
                new();

        [ObservableProperty]
        private WeldControlTowerMetrics metrics =
            new();

        [ObservableProperty]
        private List<ControlTowerWeldItem>
            visibleWelds =
                new();

        [ObservableProperty]
        private ControlTowerWeldItem? selectedWeld;

        public WeldControlTowerViewModel()
        {
            Load();
        }

        private void Load()
        {
            var welds =
                _weldRepository.GetAll();

            Metrics =
                _service.Generate(
                    welds.ToList());

            VisibleWelds =
                new List<ControlTowerWeldItem>();
        }

        [RelayCommand]
        private void ShowDraft()
        {
            VisibleWelds =
                Metrics.FilteredWelds
                    .Where(x =>
                        x.WorkflowStatus == "Draft")
                    .ToList();
        }

        [RelayCommand]
        private void ShowNdtPending()
        {
            VisibleWelds =
                Metrics.FilteredWelds
                    .Where(x =>
                        x.WorkflowStatus == "NdtPending")
                    .ToList();
        }

        [RelayCommand]
        private void ShowRepair()
        {
            VisibleWelds =
                Metrics.FilteredWelds
                    .Where(x =>
                        x.WorkflowStatus == "RepairRequired")
                    .ToList();
        }

        [RelayCommand]
        private void ShowReleased()
        {
            VisibleWelds =
                Metrics.FilteredWelds
                    .Where(x =>
                        x.WorkflowStatus == "Released")
                    .ToList();
        }

        [RelayCommand]
        private void OpenSelectedWeld()
        {
            if (SelectedWeld == null)
                return;

            WeakReferenceMessenger.Default.Send(
                new WeldNavigationMessage
                {
                    WeldId =
                        SelectedWeld.WeldId
                });
        }
    }
}
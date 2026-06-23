using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProductionControlTowerViewModel
        : ObservableObject
    {
        private readonly ProductionControlTowerService _service;

        [ObservableProperty]
        private int readyOrders;

        [ObservableProperty]
        private int runningOrders;

        [ObservableProperty]
        private int blockedOrders;

        [ObservableProperty]
        private int completedToday;

        [ObservableProperty]
        private double capacityLoad;

        [ObservableProperty]
        private int deadlineRisks;

        [ObservableProperty]
        private int materialShortages;

        [ObservableProperty]
        private int delayedWorkOrders;

        [ObservableProperty]
        private int overdueWorkOrders;

        [ObservableProperty]
        private int activeRepairs;

        [ObservableProperty]
        private int overdueReservations;

        [ObservableProperty]
        private int bottleneckCount;

        [ObservableProperty]
        private int totalWorkOrders;

        [ObservableProperty]
        private int readyToStartToday;

        [ObservableProperty]
        private int highPriorityOrders;

        [ObservableProperty]
        private int completedThisWeek;

        [ObservableProperty]
        private double productionEfficiency;

        [ObservableProperty]
        private int healthScore;

        [ObservableProperty]
        private ObservableCollection<ProductionAlert>
            alerts = new();

        private string _systemStatus = "Healthy";

        public string SystemStatus
        {
            get => _systemStatus;
            set => SetProperty(ref _systemStatus, value);
        }

        public ProductionControlTowerViewModel()
        {
            _service =
                new ProductionControlTowerService();
        }

        public void Load(int deadlineRiskCount = 0)
        {
            var data =
                _service.GetControlTower();

            ReadyOrders =
                data.ReadyOrders;

            RunningOrders =
                data.RunningOrders;

            BlockedOrders =
                data.BlockedOrders;

            CompletedToday =
                data.CompletedToday;

            CapacityLoad =
                data.CapacityLoad;

            DeadlineRisks =
                deadlineRiskCount;

            OverdueWorkOrders =
                data.OverdueWorkOrders;

            MaterialShortages =
                data.MaterialShortages;

            DelayedWorkOrders =
                data.DelayedWorkOrders;

            ActiveRepairs =
                data.ActiveRepairs;

            OverdueReservations =
                data.OverdueReservations;

            BottleneckCount =
                data.BottleneckCount;

            TotalWorkOrders =
                data.TotalWorkOrders;

            ReadyToStartToday =
                data.ReadyToStartToday;

            OverdueWorkOrders =
                data.OverdueWorkOrders;

            HighPriorityOrders =
                data.HighPriorityOrders;

            CompletedThisWeek =
                data.CompletedThisWeek;

            ProductionEfficiency =
                data.ProductionEfficiency;

            HealthScore =
                data.HealthScore;

            Alerts =
                new ObservableCollection<ProductionAlert>(
                    data.Alerts);

            CalculateStatus();

            OnPropertyChanged(nameof(IsHealthy));
            OnPropertyChanged(nameof(IsCapacityWarning));
            OnPropertyChanged(nameof(IsCapacityCritical));
        }

        public void CalculateStatus()
        {
            if (
                BlockedOrders > 0 ||
                MaterialShortages > 0 ||
                OverdueWorkOrders > 0)
            {
                SystemStatus = "Critical";
            }
            else if (
                CapacityLoad >= 100 ||
                DeadlineRisks > 0 ||
                BottleneckCount > 0)
            {
                SystemStatus = "Warning";
            }
            else
            {
                SystemStatus = "Healthy";
            }
        }

        public bool IsHealthy =>
            CapacityLoad < 80;

        public bool IsCapacityWarning =>
            CapacityLoad >= 80 &&
            CapacityLoad < 100;

        public bool IsCapacityCritical =>
            CapacityLoad >= 100;
    }
}
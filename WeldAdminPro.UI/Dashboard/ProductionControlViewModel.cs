using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Core.Services.Risk;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.ViewModels.Dashboard
{
	public partial class ProductionControlViewModel : ObservableObject
	{
        public ObservableCollection<ProductionQueueItem>
            ProductionQueue
        { get; set; }
        = new();

        public ObservableCollection<ProductionCapacityForecast>
            CapacityForecast
        { get; set; }
        = new();

        public ObservableCollection<WeldAdminPro.Core.Models.DeadlineRisk>
        DeadlineRisks
        { get; set; }
        = new();

        [ObservableProperty] private int totalWorkOrders;

        [ObservableProperty] private int readyToStartToday;

        [ObservableProperty] private int overdueWorkOrders;

        [ObservableProperty] private int highPriorityOrders;

        private readonly ProductionSchedulingService _schedulingService;

		private readonly ProductionCapacityService _capacityService;

		private readonly DeadlineRiskDetectionService _riskService;

        public ProductionControlViewModel()
        {
            var repo = new WorkOrderRepository();

            _schedulingService =
                new ProductionSchedulingService();

            _capacityService =
                new ProductionCapacityService(repo);

            _riskService =
                new DeadlineRiskDetectionService();

            Refresh();
        }

        public void Refresh()
        {
            var workOrders =
                new WorkOrderRepository()
                    .GetAll()
                    .ToList();

            if (ProductionQueue == null || ProductionQueue.Count == 0)
            {
                ProductionQueue =
                    new ObservableCollection<ProductionQueueItem>(
                        _schedulingService.BuildQueue());
            }

            CapacityForecast =
                new ObservableCollection<ProductionCapacityForecast>(
                    _capacityService.GetCapacityForecast());

            DeadlineRisks =
                new ObservableCollection<WeldAdminPro.Core.Models.DeadlineRisk>(
                    _riskService.GetDeadlineRisks(workOrders));

            OnPropertyChanged(nameof(ProductionQueue));
            OnPropertyChanged(nameof(CapacityForecast));
            OnPropertyChanged(nameof(DeadlineRisks));
        }
    }
}
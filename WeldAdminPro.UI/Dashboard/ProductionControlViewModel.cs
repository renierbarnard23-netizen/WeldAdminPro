using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Core.Services.Risk;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.UI.ViewModels.Dashboard
{
	public partial class ProductionControlViewModel : ObservableObject
	{
		public ObservableCollection<ProductionQueueItem> ProductionQueue { get; set; }

		public ObservableCollection<ProductionCapacityForecast> CapacityForecast { get; set; }

		public ObservableCollection<WeldAdminPro.Core.Models.DeadlineRisk> DeadlineRisks { get; set; }

		private readonly ProductionSchedulingService _schedulingService;

		private readonly ProductionCapacityService _capacityService;

		private readonly DeadlineRiskDetectionService _riskService;

		public ProductionControlViewModel()
		{
			var repo = new WorkOrderRepository();

			_schedulingService = new ProductionSchedulingService();

			_capacityService = new ProductionCapacityService(repo);

			_riskService = new DeadlineRiskDetectionService();

			ProductionQueue =
				new ObservableCollection<ProductionQueueItem>(
					_schedulingService.BuildQueue());

			CapacityForecast =
				new ObservableCollection<ProductionCapacityForecast>(
					_capacityService.GetCapacityForecast());

			DeadlineRisks =
	new ObservableCollection<WeldAdminPro.Core.Models.DeadlineRisk>(
		_riskService.GetDeadlineRisks(new WorkOrderRepository().GetAll()));
		}
	}
}
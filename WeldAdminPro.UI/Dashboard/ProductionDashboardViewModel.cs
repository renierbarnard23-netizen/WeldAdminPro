using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels.Dashboard
{
	public class ProductionDashboardViewModel
	{
		public ObservableCollection<ProductionQueueItem> ProductionQueue { get; }
		public ObservableCollection<ProductionCapacityForecast> CapacityForecast { get; }
		public ObservableCollection<ProductionGanttItem> Timeline { get; }

		public ProductionDashboardViewModel()
		{
			var repo = new WorkOrderRepository();

			var scheduling = new ProductionSchedulingService();
			var capacity = new ProductionCapacityService(repo);
			var gantt = new ProductionGanttService(repo);

			ProductionQueue =
				new ObservableCollection<ProductionQueueItem>(
					scheduling.BuildQueue());

			CapacityForecast =
				new ObservableCollection<ProductionCapacityForecast>(
					capacity.GetCapacityForecast());

			Timeline =
				new ObservableCollection<ProductionGanttItem>(
					gantt.GetTimeline());
		}
	}
}
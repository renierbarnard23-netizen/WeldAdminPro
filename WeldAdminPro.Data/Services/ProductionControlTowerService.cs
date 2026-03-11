using System;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionControlTowerService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionControlTowerService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public ProductionControlTowerModel GetControlTower()
		{
			var repo = new WorkOrderRepository();
			var workOrders = repo.GetAll().ToList();

			var model = new ProductionControlTowerModel();

			model.ReadyOrders =
				workOrders.Count(w => w.Status == WorkOrderStatus.Ready);

			model.RunningOrders =
				workOrders.Count(w => w.Status == WorkOrderStatus.InProduction);

			model.CompletedToday =
				workOrders.Count(w =>
					w.Status == WorkOrderStatus.Completed &&
					w.CompletedOn?.Date == DateTime.Today);

			model.BlockedOrders = 0; // will connect to shortage engine later

			model.DeadlineRisks = 0; // placeholder until risk engine added

			double totalCapacityPerDay = 32;

			double scheduledHours =
				workOrders
				.Where(w => w.Status == WorkOrderStatus.InProduction)
				.Sum(w => w.EstimatedHours);

			model.CapacityLoad =
				totalCapacityPerDay == 0
				? 0
				: (scheduledHours / totalCapacityPerDay) * 100;

			return model;
		}
	}
}
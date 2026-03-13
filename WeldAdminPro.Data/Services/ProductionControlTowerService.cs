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
			var workOrders = _workOrderRepository.GetAll().ToList();

			var model = new ProductionControlTowerModel();

			model.ReadyOrders =
				workOrders.Count(w => w.Status == WorkOrderStatus.Ready);

			model.RunningOrders =
				workOrders.Count(w => w.Status == WorkOrderStatus.InProduction);

			model.CompletedToday =
				workOrders.Count(w =>
					w.Status == WorkOrderStatus.Completed &&
					w.CompletedOn?.Date == DateTime.Today);

			model.BlockedOrders = 0; // connect shortage engine later

			model.DeadlineRisks = 0; // connect risk engine later

			double totalCapacityPerDay = 32;

			double scheduledHours =
				workOrders
				.Where(w => w.Status == WorkOrderStatus.InProduction)
				.Sum(w => w.EstimatedHours);

			model.CapacityLoad =
				totalCapacityPerDay <= 0
				? 0
				: (scheduledHours / totalCapacityPerDay) * 100;

			return model;
		}
	}
}
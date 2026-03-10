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
			var orders = _workOrderRepository.GetAll().ToList();

			var today = DateTime.Today;

			int ready = orders.Count(o => o.Status == WorkOrderStatus.Ready);

			int running = orders.Count(o => o.Status == WorkOrderStatus.InProduction);

			int completed = orders.Count(o => o.Status == WorkOrderStatus.Completed);

			int deadlineRisks = orders.Count(o =>
				o.Status != WorkOrderStatus.Completed &&
				o.DueDate.HasValue &&
				o.DueDate.Value.Date <= today.AddDays(2));

			int totalOrders = orders.Count;

			int activeOrders = ready + running;

			double capacityLoad = totalOrders == 0
				? 0
				: Math.Round((double)activeOrders / totalOrders * 100, 1);

			return new ProductionControlTowerModel
			{
				ReadyOrders = ready,
				RunningOrders = running,
				BlockedOrders = 0, // not implemented yet
				CompletedToday = completed,
				CapacityLoad = capacityLoad,
				DeadlineRisks = deadlineRisks
			};
		}
	}
}
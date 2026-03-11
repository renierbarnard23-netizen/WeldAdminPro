using System;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionThroughputService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionThroughputService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public ProductionThroughputModel GetThroughput()
		{
			var orders = _workOrderRepository.GetAll().ToList();

			var completed = orders
				.Where(o => o.Status == WorkOrderStatus.Completed)
				.ToList();

			var running = orders
				.Where(o => o.Status == WorkOrderStatus.InProduction)
				.ToList();

			var ready = orders
				.Where(o => o.Status == WorkOrderStatus.Ready)
				.ToList();

			int totalOrders = orders.Count;

			double completionRate = totalOrders == 0
				? 0
				: Math.Round((double)completed.Count / totalOrders * 100, 1);

			return new ProductionThroughputModel
			{
				CompletedToday = completed.Count(o =>
					o.CompletedOn.HasValue &&
					o.CompletedOn.Value.Date == DateTime.Today),
			};
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionBottleneckDetectionService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionBottleneckDetectionService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public List<ProductionBottleneckModel> DetectBottlenecks()
		{
			var bottlenecks = new List<ProductionBottleneckModel>();

			var orders = _workOrderRepository.GetAll().ToList();

			var today = DateTime.Today;

			// Deadline risks

			var deadlineRisks = orders
				.Where(o =>
					o.Status != WorkOrderStatus.Completed &&
					o.DueDate.HasValue &&
					o.DueDate.Value.Date <= today.AddDays(2))
				.ToList();

			foreach (var order in deadlineRisks)
			{
				bottlenecks.Add(new ProductionBottleneckModel
				{
					WorkOrderNumber = order.WorkOrderNumber,
					BottleneckType = "Deadline Risk",
					Description = "Work order due within 48 hours",
					Severity = "High"
				});
			}

			// Queue congestion

			var readyOrders = orders
				.Where(o => o.Status == WorkOrderStatus.Ready)
				.ToList();

			if (readyOrders.Count > 5)
			{
				bottlenecks.Add(new ProductionBottleneckModel
				{
					WorkOrderNumber = "-",
					BottleneckType = "Queue Congestion",
					Description = $"Too many ready work orders ({readyOrders.Count}) waiting to start",
					Severity = "Medium"
				});
			}

			return bottlenecks;
		}
	}
}
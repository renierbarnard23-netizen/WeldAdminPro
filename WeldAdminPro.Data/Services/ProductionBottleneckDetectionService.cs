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
		private readonly WorkOrderShortageDetectionService _shortageService;

		public ProductionBottleneckDetectionService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_shortageService = new WorkOrderShortageDetectionService();
		}

		public List<ProductionBottleneckModel> DetectBottlenecks()
		{
			var bottlenecks = new List<ProductionBottleneckModel>();

			var orders = _workOrderRepository.GetAll().ToList();

			var today = DateTime.Today;

			//------------------------------------------------
			// 1️⃣ MATERIAL SHORTAGES
			//------------------------------------------------

			var shortages = _shortageService.DetectShortages();

			foreach (var shortage in shortages)
			{
				bottlenecks.Add(new ProductionBottleneckModel
				{
					WorkOrderNumber = shortage.WorkOrderNumber,
					BottleneckType = "Material Shortage",
					Description = $"Missing material: {shortage.ItemCode}",
					Severity = "High",
					SuggestedAction = $"Order or reserve material: {shortage.ItemCode}"
				});
			}

			//------------------------------------------------
			// 2️⃣ DEADLINE RISKS
			//------------------------------------------------

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
					Severity = "High",
					SuggestedAction = "Prioritize immediately to avoid delay"
				});
			}

			//------------------------------------------------
			// 3️⃣ QUEUE CONGESTION
			//------------------------------------------------

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
					Severity = "Medium",
					SuggestedAction = "Start highest priority work order"
				});
			}

			//------------------------------------------------
			// 4️⃣ CAPACITY OVERLOAD
			//------------------------------------------------

			var capacityService = new ProductionCapacityService(_workOrderRepository);

			var forecast = capacityService.GetCapacityForecast();

			foreach (var day in forecast.Where(f => f.LoadPercentage > 100))
			{
				bottlenecks.Add(new ProductionBottleneckModel
				{
					WorkOrderNumber = "-",
					BottleneckType = "Capacity Overload",
					Description = $"Production exceeds capacity on {day.Date:dd MMM}",
					Severity = "High",
					SuggestedAction = "Reschedule or split workload"
				});
			}

			return bottlenecks;
		}
	}
}
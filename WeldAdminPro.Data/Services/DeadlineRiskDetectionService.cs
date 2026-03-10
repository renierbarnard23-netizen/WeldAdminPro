using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class DeadlineRiskDetectionService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly ProductionCapacityService _capacityService;

		public DeadlineRiskDetectionService(
			WorkOrderRepository workOrderRepository,
			ProductionCapacityService capacityService)
		{
			_workOrderRepository = workOrderRepository;
			_capacityService = capacityService;
		}

		public List<DeadlineRisk> DetectRisks()
		{
			var results = new List<DeadlineRisk>();

			var workOrders = _workOrderRepository.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.ToList();

			var capacityForecast = _capacityService.GetCapacityForecast(14);

			foreach (var wo in workOrders)
			{
				var dueDate = wo.DueDate ?? wo.StartDate.AddDays(3);

				var capacityWindow = capacityForecast
					.Where(f => f.Date >= wo.StartDate && f.Date <= dueDate)
					.ToList();

				double availableHours = capacityWindow
					.Sum(d => d.CapacityHours - d.ScheduledHours);

				double shortage = wo.EstimatedHours - availableHours;

				if (shortage > 0)
				{
					results.Add(new DeadlineRisk
					{
						WorkOrderNumber = wo.WorkOrderNumber,
						StartDate = wo.StartDate,
						DueDate = dueDate,
						EstimatedHours = wo.EstimatedHours,
						AvailableHours = availableHours,
						HoursShortage = shortage,
						RiskLevel = shortage > wo.EstimatedHours * 0.5
							? "HIGH"
							: "MEDIUM"
					});
				}
			}

			return results;
		}
	}
}
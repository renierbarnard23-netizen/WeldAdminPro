using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderAutoSchedulerService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly ProductionCapacityService _capacityService;

		public WorkOrderAutoSchedulerService(
			WorkOrderRepository workOrderRepository,
			ProductionCapacityService capacityService)
		{
			_workOrderRepository = workOrderRepository;
			_capacityService = capacityService;
		}

		public List<AutoScheduleResult> BalanceSchedule(int forecastDays = 14)
		{
			var results = new List<AutoScheduleResult>();

			var forecasts = _capacityService.GetCapacityForecast(forecastDays);

			var workOrders = _workOrderRepository.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.OrderBy(w => w.Priority)
				.ToList();

			foreach (var overloadedDay in forecasts.Where(f => f.LoadPercentage > 100))
			{
				var dayOrders = workOrders
					.Where(w => w.StartDate.Date == overloadedDay.Date.Date)
					.OrderByDescending(w => w.EstimatedHours)
					.ToList();

				foreach (var wo in dayOrders)
				{
					var targetDay = forecasts
						.Where(f => f.LoadPercentage < 80)
						.OrderBy(f => f.LoadPercentage)
						.FirstOrDefault();

					if (targetDay == null)
						break;

					var originalDate = wo.StartDate;

					wo.StartDate = targetDay.Date;

					results.Add(new AutoScheduleResult
					{
						WorkOrderNumber = wo.WorkOrderNumber,
						OriginalDate = originalDate,
						NewDate = targetDay.Date,
						HoursMoved = wo.EstimatedHours
					});

					_workOrderRepository.Update(wo);
				}
			}

			return results;
		}
	}
}
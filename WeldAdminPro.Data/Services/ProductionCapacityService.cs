using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionCapacityService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionCapacityService(WorkOrderRepository workOrderRepository)
		{
			_workOrderRepository = workOrderRepository;
		}

		public List<ProductionCapacityForecast> GetCapacityForecast(
			int days = 14,
			double hoursPerDay = 8,
			int stations = 4)
		{
			var forecasts = new List<ProductionCapacityForecast>();

			double dailyCapacity = hoursPerDay * stations;

			var workOrders = _workOrderRepository.GetAll()
					.Where(w => w.Status != WorkOrderStatus.Completed)
					.ToList();

			for (int i = 0; i < days; i++)
			{
				var date = DateTime.Today.AddDays(i);

				var scheduledHours = workOrders
					.Where(w => w.StartDate.Date == date.Date)
					.Sum(w => w.EstimatedHours);

				var load = dailyCapacity == 0
					? 0
					: (scheduledHours / dailyCapacity) * 100;

				forecasts.Add(new ProductionCapacityForecast
				{
					Date = date,
					CapacityHours = dailyCapacity,
					ScheduledHours = scheduledHours,
					LoadPercentage = Math.Round(load, 1)
				});
			}

			return forecasts;
		}
	}
}
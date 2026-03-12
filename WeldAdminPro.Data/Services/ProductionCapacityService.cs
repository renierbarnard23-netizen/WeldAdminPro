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
			int stations = 25)
		{
			var forecasts = new List<ProductionCapacityForecast>();

			var settingsRepo = new ProductionSettingsRepository();

			var settings = settingsRepo.Get();

			double dailyCapacity = settings.DailyCapacity;

			var scheduleService = new ProductionScheduleService();
			var schedule = scheduleService.GetSchedule();

			for (int i = 0; i < days; i++)
			{
				var date = DateTime.Today.AddDays(i);

				var scheduledHours = schedule
					.Where(s => s.StartDate.Date == date.Date)
					.Sum(s => (s.EndDate - s.StartDate).TotalHours);

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
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionDelayPredictionService
	{
		public List<ProductionDelayPrediction> PredictDelays()
		{
			var repo = new WorkOrderRepository();
			var scheduler = new ProductionScheduleService();

			var schedule = scheduler.GetSchedule();
			var orders = repo.GetAll();

			var predictions = new List<ProductionDelayPrediction>();

			foreach (var item in schedule)
			{
				var order = orders
					.FirstOrDefault(o => o.WorkOrderNumber == item.WorkOrderNumber);

				if (order?.DueDate == null)
					continue;

				if (item.EndDate > order.DueDate)
				{
					var daysLate = (item.EndDate - order.DueDate.Value).TotalDays;

					predictions.Add(new ProductionDelayPrediction
					{
						WorkOrderNumber = item.WorkOrderNumber,
						ScheduledEnd = item.EndDate,
						DueDate = order.DueDate.Value,
						DaysLate = Math.Round(daysLate, 1),
						RiskLevel = daysLate > 2 ? "Critical" : "Warning"
					});
				}
			}

			return predictions;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionScheduleService
	{
		public List<ProductionScheduleItem> GetSchedule()
		{
			var repo = new WorkOrderRepository();

			var workOrders = repo.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.OrderBy(w => w.CreatedOn)
				.ToList();

			DateTime currentStart = DateTime.Today;

			var result = new List<ProductionScheduleItem>();

			foreach (var wo in workOrders)
			{
				double hours = wo.EstimatedHours <= 0 ? 8 : wo.EstimatedHours;

				double days = hours / 8.0;

				var schedule = new ProductionScheduleItem
				{
					WorkOrderNumber = wo.WorkOrderNumber,
					StartDate = currentStart,
					EndDate = currentStart.AddDays(days)
				};

				result.Add(schedule);

				currentStart = schedule.EndDate;
			}

			return result;
		}
	}
}
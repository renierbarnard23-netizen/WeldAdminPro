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
			var shortageService = new WorkOrderShortageDetectionService();

			var shortages = shortageService.DetectShortages()
				.Select(s => s.WorkOrderNumber)
				.Distinct()
				.ToList();

			var workOrders = repo.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.Where(w => !shortages.Contains(w.WorkOrderNumber))
				.GroupBy(w => w.WorkOrderNumber)
				.Select(g => g.First())
				.OrderBy(w => w.CreatedOn)
				.ToList();

			if (workOrders.Count == 0)
				return new List<ProductionScheduleItem>();

			DateTime currentStart = workOrders
				.Select(w => w.PlannedStartDate ?? DateTime.Today)
				.DefaultIfEmpty(DateTime.Today)
				.Min();

			var result = new List<ProductionScheduleItem>();

			foreach (var wo in workOrders)
			{

                System.Diagnostics.Debug.WriteLine(
					$"{wo.WorkOrderNumber} | Estimated Hours = {wo.EstimatedHours}");

                double hours = wo.EstimatedHours > 0 ? wo.EstimatedHours : 8;
                double days =
					Math.Max(
						1,
						Math.Ceiling(hours / 8.0));

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
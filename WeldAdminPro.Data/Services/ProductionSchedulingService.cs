using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionSchedulingService
	{
		private readonly WorkOrderRepository _repository;

        public ProductionSchedulingService()
        {
            _repository =
                new WorkOrderRepository();

            _repository.RepairPriorities();
        }

        public List<ProductionQueueItem> BuildQueue()
        {
            var workOrders = _repository
                .GetAll()
                .Where(w => w.Status != WorkOrderStatus.Completed)
                .ToList();

            var queue = workOrders
                .Select(wo => new ProductionQueueItem
                {
                    Id = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    Priority = wo.Priority,

                    // Single source of truth
                    Status = wo.Status.ToString(),

                    StartDate = wo.PlannedStartDate?.ToShortDateString() ?? "",
                    DueDate = wo.DueDate?.ToShortDateString() ?? "",

                    Deadline = wo.DueDate ?? DateTime.Today.AddDays(7),

                    EstimatedHours =
                        wo.EstimatedHours > 0
                            ? wo.EstimatedHours
                            : 8
                })
                .OrderBy(q => q.Priority)
                .ToList();

            foreach (var q in queue)
            {
                Debug.WriteLine(
                    $"{q.WorkOrderNumber} : {q.Priority}");
            }

            return queue;
        }
        public void UpdateQueueOrder(List<ProductionQueueItem> queue)
		{
			int priority = 1;

			foreach (var item in queue)
			{
				_repository.UpdatePriority(
                    item.WorkOrderNumber, 
                    priority);

				priority++;
			}
		}
	}
}
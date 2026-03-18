using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionSchedulingService
	{
		private readonly WorkOrderRepository _repository;
		private readonly WorkOrderStatusService _statusService;

		public ProductionSchedulingService()
		{
			_repository = new WorkOrderRepository();
			_statusService = new WorkOrderStatusService();
		}

		public List<ProductionQueueItem> BuildQueue()
		{
			var workOrders = _repository
				.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.ToList();

			var statuses = _statusService.GetStatuses();

			var queue = workOrders
				.Select(wo =>
				{
					var status = statuses
						.FirstOrDefault(s => s.WorkOrderNumber == wo.WorkOrderNumber);


					return new ProductionQueueItem
					{
						Id = wo.Id,
						WorkOrderNumber = wo.WorkOrderNumber,
						Priority = wo.Priority,
						Status = status?.Status.ToString() ?? "Unknown",
						StartDate = wo.PlannedStartDate?.ToShortDateString() ?? "",
						DueDate = wo.DueDate?.ToShortDateString() ?? "",

						// 🔥 ADD THESE (CRITICAL FIX)
						Deadline = wo.DueDate ?? DateTime.Today.AddDays(7),

						EstimatedHours = wo.EstimatedHours > 0
		? wo.EstimatedHours
		: 8 // fallback
					};
				})
				.OrderBy(q => q.Priority)
				.ToList();

			return queue;
		}
		public void UpdateQueueOrder(List<ProductionQueueItem> queue)
		{
			int priority = 1;

			foreach (var item in queue)
			{
				_repository.UpdatePriority(item.WorkOrderNumber, priority);
				priority++;
			}
		}
	}
}
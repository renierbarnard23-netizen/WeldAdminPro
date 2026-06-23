using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionGanttService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionGanttService(WorkOrderRepository workOrderRepository)
		{
			_workOrderRepository = workOrderRepository;
		}

		public List<ProductionGanttItem> GetTimeline()
		{
			var orders = _workOrderRepository.GetAll();

			if (!orders.Any())
				return new List<ProductionGanttItem>();

			var minDate = orders.Min(x => x.DueDate ?? DateTime.Today);

			return orders.Select(o =>
			{
				var due = o.DueDate ?? DateTime.Today;

				// Temporary estimated start date
				var start = due.AddDays(-5);

				return new ProductionGanttItem
				{
					WorkOrderNumber = o.WorkOrderNumber ?? "N/A",

					StartDate = start,
					DueDate = due,

					Status = o.Status.ToString(),

					StartOffset = (start - minDate).TotalDays * 40,
					Duration = (due - start).TotalDays * 40
				};
			}).ToList();
		}
	}
}
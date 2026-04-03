using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Planning
{
	public class ProductionPlannerService : IProductionPlanner
	{
		public PlanningResult GeneratePlan(PlanningContext context)
		{
			var result = new PlanningResult();

			var now = DateTime.Now;

			var sorted = context.WorkOrders
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.OrderBy(w => w.DueDate)
				.ToList();

			foreach (var wo in sorted)
			{
				var materials = context.GetMaterials(wo.Id);

				// 🔥 FIX: Use correct field (CHANGE THIS if needed)
				bool hasMaterialRisk = materials.Any(m =>
					context.GetStock(m.ItemCode ?? "") < m.RequiredQuantity
				);

				if (hasMaterialRisk)
					continue;

				double duration = wo.EstimatedHours;

				var start = now;
				var end = start.AddHours(duration);

				bool isLate = false;

				// 🔥 FIX: DueDate is DateTime?
				if (wo.DueDate.HasValue)
				{
					var due = wo.DueDate.Value;

					if (end > due)
					{
						isLate = true;
						result.TotalDelayHours += (end - due).TotalHours;
						result.LateJobs++;
					}
				}

				result.Sequence.Add(new PlannedWorkOrder
				{
					WorkOrderId = wo.Id,
					WorkOrderNumber = wo.WorkOrderNumber,
					Start = start,
					End = end,
					IsLate = isLate
				});

				now = end;
			}

			return result;
		}
	}
}
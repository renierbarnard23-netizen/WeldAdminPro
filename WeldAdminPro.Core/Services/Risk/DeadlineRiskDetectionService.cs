using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services.Interfaces;

namespace WeldAdminPro.Core.Services.Risk
{
	public class DeadlineRiskDetectionService : IRiskService
	{
		public List<WeldAdminPro.Core.Models.DeadlineRisk> GetDeadlineRisks(IEnumerable<WorkOrder> workOrders)
		{
			if (workOrders == null)
				return new List<WeldAdminPro.Core.Models.DeadlineRisk>();

			var now = DateTime.Now;

			var risks = new List<WeldAdminPro.Core.Models.DeadlineRisk>();

			foreach (var order in workOrders)
			{
				// 🔥 ONLY evaluate active work
				if (order.Status == WorkOrderStatus.Completed)
					continue;

				// 🔥 Must have a due date
				if (order.DueDate == null)
					continue;

				var deadline = order.DueDate.Value;

				// 🔥 Ignore past deadlines
				if (deadline <= now)
					continue;

				double estimated = order.EstimatedHours;
				double completed = 0; // temp

				double remainingHours = Math.Max(estimated - completed, 0);
				double availableHours = (deadline - now).TotalHours;

				bool isHardRisk = remainingHours > availableHours;

				// 🔥 NEW: Soft risk (deadline within 48h)
				bool isSoftRisk = availableHours <= 48;

				bool isAtRisk = isHardRisk || isSoftRisk;

				if (!isAtRisk)
					continue;

				double delayHours = remainingHours - availableHours;

                risks.Add(new DeadlineRisk
                {
                    WorkOrderId = order.Id,
                    WorkOrderNumber = order.WorkOrderNumber,
                    Deadline = deadline,
                    RemainingHours = remainingHours,
                    AvailableHours = Math.Max(availableHours, 0),
                    IsAtRisk = true,
                    DelayHours = Math.Max(delayHours, 0),

                    RiskLevel =
        delayHours > 24
            ? "High"
            : availableHours <= 48
                ? "Medium"
                : "Low",

                    Reason =
        isHardRisk
            ? "Insufficient production hours before due date."
            : "Deadline within 48 hours."
                });
            }

			return risks
				.OrderByDescending(r => r.IsAtRisk)
				.ThenBy(r => r.Deadline)
				.ToList();
		}
	}
}
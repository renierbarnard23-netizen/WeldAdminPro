using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;

public class AutoPriorityService
{
	public List<ProductionQueueItem> ReorderQueue(
		List<ProductionQueueItem> workOrders,
		List<ProductionBottleneckModel> bottlenecks)
	{
		var scores = new Dictionary<string, int>();

		foreach (var wo in workOrders)
		{
			int score = 0;

			// 🔥 Base priority
			score += wo.Priority * 10;

			// 🔥 Status boost
			if (wo.Status == "Ready") score += 50;
			if (wo.Status == "Blocked") score -= 100;

			// 🔥 Bottlenecks
			var block = bottlenecks.FirstOrDefault(b => b.WorkOrderNumber == wo.WorkOrderNumber);

			if (block != null)
			{
				if (block.Severity == "High") score -= 80;
				else if (block.Severity == "Medium") score -= 40;
			}
			else
			{
				score += 20;
			}

			// 🔥 Due date urgency (STRONGER)
			if (DateTime.TryParse(wo.DueDate, out var dueDate))
			{
				var daysLeft = (dueDate - DateTime.Now).TotalDays;

				if (daysLeft <= 1) score += 100;
				else if (daysLeft <= 3) score += 60;
				else if (daysLeft <= 7) score += 30;
			}

			scores[wo.WorkOrderNumber] = score;
		}

		return workOrders
			.OrderByDescending(w => scores[w.WorkOrderNumber])
			.ToList();
	}
}

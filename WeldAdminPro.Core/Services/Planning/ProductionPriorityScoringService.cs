using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Core.Services.Planning
{
	public class ProductionPriorityScoringService
	{
		public List<ProductionQueueItem> Score(
			List<ProductionQueueItem> queue,
			List<ProductionBottleneckModel> bottlenecks)
		{
			foreach (var wo in queue)
			{
				double score = 0;

				// 🔥 1. Deadline urgency
				var daysToDeadline = (wo.Deadline - DateTime.Today).TotalDays;

				if (daysToDeadline <= 0)
					score += 100;
				else
					score += 50 / daysToDeadline;

				// 🔥 2. Duration (short jobs first)
				if (wo.EstimatedHours < 8)
					score += 10;

				// 🔥 3. Already late
				if (wo.IsLate)
					score += 50;

				// 🔥 4. Strong urgency boost
				if (daysToDeadline < 2)
					score += 50;
				else if (daysToDeadline < 5)
					score += 20;

				// 🔥 5. Bottleneck penalty
				if (bottlenecks.Any(b => b.Resource == wo.RequiredResource))
				{
					score -= 20;
				}

				wo.PriorityScore = score;
			}

			return queue
				.OrderByDescending(q => q.PriorityScore)
				.ToList();
		}
	}
}
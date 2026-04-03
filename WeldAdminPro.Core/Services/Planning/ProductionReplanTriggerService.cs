using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Core.Services.Planning
{
	public class ProductionReplanTriggerService
	{
		public bool ShouldReplan(
			List<ProductionQueueItem> queue,
			List<ProductionDelayPrediction> delays,
			List<ProductionBottleneckModel> bottlenecks)
		{
			// 🔥 RULE 1: Late jobs
			if (delays.Any(d => d.DelayDays > 0))
				return true;

			// 🔥 RULE 2: Bottlenecks
			if (bottlenecks.Any())
				return true;

			// 🔥 RULE 3: Blocked jobs in queue
			if (queue.Any(q => q.Status == "Blocked"))
				return true;

			// 🔥 RULE 4: Idle capacity (nothing running)
			if (!queue.Any(q => q.Status == "InProduction"))
				return true;

			return false;
		}
	}
}
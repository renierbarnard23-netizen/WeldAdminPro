using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Services.Planning;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionReplanningService
	{
		private readonly ProductionOptimizerService _optimizer;

		public ProductionReplanningService()
		{
			_optimizer = new ProductionOptimizerService();
		}

		public List<ProductionQueueItem> Replan(List<ProductionQueueItem> queue)
		{
			var optimizer = new ProductionOptimizer();

			var result = optimizer.Optimize(queue, 8);

			// Map back using WorkOrderNumber (NOT SourceWorkOrder)
			var replanned = result.BestSequence
				.Select(p => queue.FirstOrDefault(q => q.WorkOrderNumber == p.WorkOrderNumber))
				.Where(q => q != null)
				.ToList();

			return replanned!;
		}
	}
}
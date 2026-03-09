using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services
{
	public class ProductionBlockService
	{
		private readonly WorkOrderShortageDetectionService _shortageService;

		public ProductionBlockService()
		{
			_shortageService = new WorkOrderShortageDetectionService();
		}

		public List<ProductionBlock> GetBlockedWorkOrders()
		{
			var shortages = _shortageService.DetectShortages();

			var blocks = shortages
	.GroupBy(s => s.WorkOrderNumber)
	.Select(g => new ProductionBlock
	{
		WorkOrderNumber = g.Key,

		BlockingItems = g
			.Select(x => $"{x.ItemCode} ({x.ShortageQuantity})")
			.ToList(),

		TotalShortage = (double)g.Sum(x => x.ShortageQuantity)
	})
	.ToList();

			return blocks;
		}
	}
}
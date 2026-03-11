using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

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

			var stockRepo = new StockRepository();
			var stockItems = stockRepo.GetAll();

			var blocks = shortages
				.GroupBy(s => s.WorkOrderNumber)
				.Select(g =>
				{
					var groupedItems = g
						.GroupBy(x => x.ItemCode)
						.Select(itemGroup =>
						{
							var stock = stockItems
								.FirstOrDefault(s => s.ItemCode == itemGroup.Key);

							var name = stock?.Description ?? itemGroup.Key;

							var totalShortage = itemGroup.Sum(x => x.ShortageQuantity);

							return $"{name} ({totalShortage})";
						})
						.ToList();

					return new ProductionBlock
					{
						WorkOrderNumber = g.Key,
						BlockingItems = groupedItems,
						TotalShortage = (double)g.Sum(x => x.ShortageQuantity)
					};
				})
				.ToList();

			return blocks;
		}
	}
}
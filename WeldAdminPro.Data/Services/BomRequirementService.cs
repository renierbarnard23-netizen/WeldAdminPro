using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class BomRequirementService
	{
		private readonly BillOfMaterialRepository _bomRepository;
		private readonly StockRepository _stockRepository;

		public BomRequirementService()
		{
			_bomRepository = new BillOfMaterialRepository();
			_stockRepository = new StockRepository();
		}

		public List<WorkOrderMaterialPlan> BuildRequirements()
		{
			var stock = _stockRepository.GetAll();
			var boms = _bomRepository.GetAll();

			var plans = new List<WorkOrderMaterialPlan>();

			foreach (var bom in boms)
			{
				var items = _bomRepository.GetItems(bom.Id);

				foreach (var item in items)
				{
					var stockItem = stock
						.FirstOrDefault(s => s.ItemCode == item.ItemCode);

					int available = stockItem?.Quantity ?? 0;

					plans.Add(new WorkOrderMaterialPlan
					{
						WorkOrderNumber = bom.ProductCode,
						ItemCode = item.ItemCode,
						Description = item.Description,
						RequiredQuantity = item.QuantityRequired,
						StockAvailable = available
					});
				}
			}

			return plans
				.OrderByDescending(p => p.Shortage)
				.ToList();
		}
	}
}
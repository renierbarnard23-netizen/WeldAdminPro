using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderMaterialPlanningService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly StockRepository _stockRepository;

		public WorkOrderMaterialPlanningService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_stockRepository = new StockRepository();
		}

		public List<WorkOrderMaterialPlan> BuildPlan()
		{
			var workOrders = _workOrderRepository.GetAll();
			var stockItems = _stockRepository.GetAll();

			var plans = new List<WorkOrderMaterialPlan>();

			foreach (var wo in workOrders)
			{
				// Example logic
				// (later we connect this to real BOM / materials list)

				foreach (var item in stockItems.Take(3))
				{
					plans.Add(new WorkOrderMaterialPlan
					{
						WorkOrderNumber = wo.WorkOrderNumber,
						ItemCode = item.ItemCode,
						Description = item.Description,
						RequiredQuantity = 50,
						StockAvailable = item.Quantity
					});
				}
			}

			return plans
				.OrderByDescending(p => p.Shortage)
				.ToList();
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
	public class SmartReorderPlannerService
	{
		public List<StockItem> GetReorderItems(IEnumerable<StockItem> items)
		{
			return items
				.Where(i =>
					i.SmartStatus == SmartStockStatus.Critical ||
					i.SmartStatus == SmartStockStatus.ReorderRequired)
				.OrderBy(i => i.SmartStatus) // Critical first
				.ThenByDescending(i => i.StockValueRisk)
				.ToList();
		}
	}
}

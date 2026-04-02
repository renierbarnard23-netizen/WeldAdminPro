using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Risk
{
	public class DependencyRiskService
	{
		public List<ProductionRisk> GetDependencyRisks(IEnumerable<WorkOrder> workOrders)
		{
			var risks = new List<ProductionRisk>();

			foreach (var order in workOrders)
			{
				if (order.Status == WorkOrderStatus.Completed)
					continue;

				if (order.Dependencies == null || !order.Dependencies.Any())
					continue;

				var blocked = order.Dependencies
					.Any(d => d.Status != WorkOrderStatus.Completed);

				if (blocked)
				{
					risks.Add(new ProductionRisk
					{
						WorkOrderId = order.Id,
						WorkOrderNumber = order.WorkOrderNumber,

						RiskType = RiskType.Dependency,

						Issue = "Blocked by dependency",
						Action = "Complete dependent work order first"
					});
				}
			}

			return risks;
		}
	}
}
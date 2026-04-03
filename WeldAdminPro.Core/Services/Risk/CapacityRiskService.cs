using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Risk
{
	public class CapacityRiskService
	{
		public List<ProductionRisk> GetCapacityRisks(IEnumerable<WorkCenter> centers)
		{
			var risks = new List<ProductionRisk>();

			foreach (var center in centers)
			{
				if (center.HoursPerDay <= 0)
					continue;

				var utilization = center.CurrentLoadHours / center.HoursPerDay;

				// 🚨 OVERLOADED
				if (utilization > 1.0)
				{
					risks.Add(new ProductionRisk
					{
						RiskType = RiskType.Capacity,
						WorkOrderNumber = center.Name,

						Issue = $"Overloaded: {center.Name}",
						Action = "Rebalance workload or add capacity"
					});
				}
				// ⚠ WARNING ZONE
				else if (utilization > 0.85)
				{
					risks.Add(new ProductionRisk
					{
						RiskType = RiskType.Capacity,
						WorkOrderNumber = center.Name,

						Issue = $"High utilization: {center.Name}",
						Action = "Monitor closely or shift work"
					});
				}
			}

			return risks;
		}
	}
}
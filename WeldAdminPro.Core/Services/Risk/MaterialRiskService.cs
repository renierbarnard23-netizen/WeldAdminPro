using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Risk
{
	public class MaterialRiskService
	{
		public List<ProductionRisk> GetMaterialRisks(
			IEnumerable<WorkOrder> workOrders,
			Func<Guid, IEnumerable<MaterialRequirement>> getMaterials,
			Func<string, double> getAvailableStock)
		{
			var risks = new List<ProductionRisk>();

			foreach (var order in workOrders)
			{
				if (order.Status == WorkOrderStatus.Completed)
					continue;

				var materials = getMaterials(order.Id);

				if (materials == null || !materials.Any())
					continue;

				foreach (var mat in materials)
				{
					double availableQty = getAvailableStock(mat.ItemCode);

					if (mat.RequiredQuantity > availableQty)
					{
						risks.Add(new ProductionRisk
						{
							WorkOrderId = order.Id,
							WorkOrderNumber = order.WorkOrderNumber,

							RiskType = RiskType.Material,

							Issue = $"Missing material: {mat.ItemCode}",
							Action = $"Order or reserve material: {mat.ItemCode}"
						});
					}
				}
			}

			return risks;
		}
	}
}
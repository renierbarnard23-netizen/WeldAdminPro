using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services.Planning;

namespace WeldAdminPro.Core.Services.Risk
{
	public class GlobalRiskService
	{
		private readonly DeadlineRiskDetectionService _deadlineService;
		private readonly MaterialRiskService _materialService;
		private readonly DependencyRiskService _dependencyService;
		private readonly CapacityRiskService _capacityRiskService = new();

		public GlobalRiskService()
		{
			_deadlineService = new DeadlineRiskDetectionService();
			_materialService = new MaterialRiskService();
			_dependencyService = new DependencyRiskService();
		}

		public List<ProductionRisk> GetAllRisks(
			IEnumerable<WorkOrder> workOrders,
				Func<Guid, IEnumerable<MaterialRequirement>> getMaterials,
				Func<string, double> getStock)
		{
			var risks = new List<ProductionRisk>();

			// 🔥 Deadline Risks
			var deadlineRisks = _deadlineService
				.GetDeadlineRisks(workOrders)
				.Select(r => new ProductionRisk
				{
					WorkOrderId = r.WorkOrderId,
					WorkOrderNumber = r.WorkOrderNumber,
					RiskType = RiskType.Deadline,
					Issue = "Work order at risk of delay",
					Action = "Prioritize immediately",
					Deadline = r.Deadline
				});

			// 🔥 Material Risks
			var materialRisks = _materialService.GetMaterialRisks(
				workOrders,
				getMaterials,
				getStock
				);

			// 🔥 Dependency Risks
			var dependencyRisks = _dependencyService.GetDependencyRisks(workOrders);

			risks.AddRange(deadlineRisks);
			risks.AddRange(materialRisks);
			risks.AddRange(dependencyRisks);

			// 🔥 CAPACITY RISKS
			var capacityEngine = new WorkCenterCapacityService();
			var centers = capacityEngine.CalculateCapacity(workOrders);

			var capacityRisks = _capacityRiskService.GetCapacityRisks(centers);

			risks.AddRange(capacityRisks);

			return risks;
		}
	}
}
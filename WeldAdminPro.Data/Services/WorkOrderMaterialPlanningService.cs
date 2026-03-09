using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderMaterialPlanningService
	{
		private readonly BomRequirementService _bomService;

		public WorkOrderMaterialPlanningService()
		{
			_bomService = new BomRequirementService();
		}

		public List<WorkOrderMaterialPlan> BuildPlan()
		{
			return _bomService.BuildRequirements();
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderShortageDetectionService
	{
		private readonly BomRequirementService _bomRequirementService;

		public WorkOrderShortageDetectionService()
		{
			_bomRequirementService = new BomRequirementService();
		}

		public List<WorkOrderMaterialShortage> DetectShortages()
		{
			var requirements = _bomRequirementService.BuildRequirements();

			var shortages = requirements
				.Where(r => r.Shortage > 0)
				.Select(r => new WorkOrderMaterialShortage
				{
					WorkOrderNumber = r.WorkOrderNumber,
					ItemCode = r.ItemCode,
					ItemName = r.Description,
					RequiredQuantity = r.RequiredQuantity,
					AvailableQuantity = r.StockAvailable,
					ShortageQuantity = r.Shortage
				})
				.ToList();

			return shortages;
		}

		// NEW METHOD (used by other intelligence services)
		public List<WorkOrderMaterialShortage> GetShortages()
		{
			return DetectShortages();
		}
	}
}
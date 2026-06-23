using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderShortageDetectionService
	{
		private readonly BomRequirementService _bomRequirementService;
		private readonly WorkOrderMaterialRepository _materialRepo;
		private readonly WorkOrderRepository _workOrderRepo;
		private readonly StockRepository _stockRepo;

		public WorkOrderShortageDetectionService()
		{
			_bomRequirementService = new BomRequirementService();
			_materialRepo = new WorkOrderMaterialRepository();
			_workOrderRepo = new WorkOrderRepository();
			_stockRepo = new StockRepository();
		}

		public List<WorkOrderMaterialShortage> DetectShortages()
		{
			var shortages = new List<WorkOrderMaterialShortage>();

			// Existing BOM shortages
			var requirements = _bomRequirementService.BuildRequirements();

			shortages.AddRange(
				requirements
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
			);

			// NEW: WorkOrderMaterials shortages
			var workOrders = _workOrderRepo.GetAll()
	.Where(w => w.Status != WorkOrderStatus.Completed)
	.ToList();

			foreach (var wo in workOrders)
			{
                var materials = _materialRepo.GetByWorkOrderId(wo.Id)
					?? Enumerable.Empty<WorkOrderMaterial>();

                Debug.WriteLine($"🔥 MATERIAL COUNT: {materials.Count()}");

                foreach (var mat in materials)
				{
					var stock = _stockRepo.GetAll()
						.FirstOrDefault(s => s.ItemCode == mat.ItemCode);

					int available = (int)Math.Floor(stock?.Quantity ?? 0);

                    if (string.IsNullOrWhiteSpace(mat.ItemCode))
                        continue;

                    if (available < mat.RequiredQuantity)
					{
						shortages.Add(new WorkOrderMaterialShortage
						{
							WorkOrderNumber = wo.WorkOrderNumber,
							ItemCode = mat.ItemCode,
							ItemName = stock?.Description ?? mat.ItemCode,
							RequiredQuantity = (decimal)mat.RequiredQuantity,
							AvailableQuantity = (decimal)available,
							ShortageQuantity = (decimal)(mat.RequiredQuantity - available)
						});
					}
				}
			}

			return shortages;
		}

		// NEW METHOD (used by other intelligence services)
		public List<WorkOrderMaterialShortage> GetShortages()
		{
			return DetectShortages();
		}
	}
}
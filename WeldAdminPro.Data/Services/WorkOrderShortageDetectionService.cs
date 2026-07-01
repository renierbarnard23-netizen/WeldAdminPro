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
        private readonly ReservedMaterialRepository _reservationRepo;
        public WorkOrderShortageDetectionService()
		{
			_bomRequirementService = new BomRequirementService();
			_materialRepo = new WorkOrderMaterialRepository();
			_workOrderRepo = new WorkOrderRepository();
			_stockRepo = new StockRepository();
            _reservationRepo = new ReservedMaterialRepository();
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

                Debug.WriteLine($"WO: {wo.WorkOrderNumber}");

                foreach (var mat in materials)
                {
                    Debug.WriteLine(
                        $"Item={mat.ItemCode}  Req={mat.RequiredQuantity}");

                    var stock =
                        _stockRepo.GetAll()
                        .FirstOrDefault(s =>
                            s.ItemCode == mat.ItemCode);

                    Debug.WriteLine(
                        $"Available={stock?.Quantity}");

                    double physical =
                        stock?.Quantity ?? 0;

                    double available =
                        _reservationRepo
                            .GetAvailableQuantity(
                                mat.ItemCode,
                                physical);

                    if (available < mat.RequiredQuantity)
                    {
                        bool alreadyAdded =
                            shortages.Any(s =>
                                s.WorkOrderNumber == wo.WorkOrderNumber &&
                                s.ItemCode == mat.ItemCode);

                        if (!alreadyAdded)
                        {
                            shortages.Add(
                                new WorkOrderMaterialShortage
                                {
                                    WorkOrderNumber = wo.WorkOrderNumber,
                                    ItemCode = mat.ItemCode,
                                    ItemName =
                                        stock?.Description
                                        ?? mat.ItemCode,
                                    RequiredQuantity =
                                        (decimal)mat.RequiredQuantity,
                                    AvailableQuantity =
                                        (decimal)available,
                                    ShortageQuantity =
                                        (decimal)
                                        (mat.RequiredQuantity
                                         - available)
                                });

                            Debug.WriteLine(
                                $"🔥 SHORTAGE FOUND ON {wo.WorkOrderNumber}");
                        }
                    }
                }
            }

            Debug.WriteLine(
					$"TOTAL SHORTAGES = {shortages.Count}");

            foreach (var s in shortages)
            {
                Debug.WriteLine(
                    $"SHORTAGE -> {s.WorkOrderNumber} | {s.ItemCode}");
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
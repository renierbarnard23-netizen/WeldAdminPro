using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionReadinessService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly WorkOrderShortageDetectionService _shortageService;

		public ProductionReadinessService(
			WorkOrderRepository workOrderRepository,
			WorkOrderShortageDetectionService shortageService)
		{
			_workOrderRepository = workOrderRepository;
			_shortageService = shortageService;
		}

		public ProductionReadinessResult Calculate()
		{
			var workOrders = _workOrderRepository.GetAll().ToList();

			var shortages = _shortageService.GetShortages();

			var blockedWorkOrders = shortages
				.Select(s => s.WorkOrderNumber)
				.Distinct()
				.ToList();

			int total = workOrders.Count;

			int blocked = blockedWorkOrders.Count;

			int ready = total - blocked;

			double percentage = total == 0
				? 100
				: (double)ready / total * 100;

			return new ProductionReadinessResult
			{
				TotalWorkOrders = total,
				ReadyWorkOrders = ready,
				BlockedWorkOrders = blocked,
				ReadinessPercentage = percentage,
				BlockedWorkOrderNumbers = blockedWorkOrders
			};
		}

        public List<WorkOrderReadiness> GetWorkOrderReadiness()
        {
            var result =
                new List<WorkOrderReadiness>();

            var workOrders =
                _workOrderRepository
                    .GetAll()
                    .ToList();

            var shortages =
                _shortageService
                    .GetShortages();

            Debug.WriteLine("===== READINESS =====");

            foreach (var shortage in shortages)
            {
                Debug.WriteLine(
                    $"SHORTAGE WO={shortage.WorkOrderNumber}");
            }

            foreach (var wo in workOrders)
            {
                Debug.WriteLine(
                    $"CHECKING {wo.WorkOrderNumber}");

                var readiness =
                    new WorkOrderReadiness
                    {
                        WorkOrderId = wo.Id,
                        WorkOrderNumber = wo.WorkOrderNumber,
                        MaterialsReady = true,
                        DependenciesReady = true,
                        Reason = "Ready"
                    };

                var woShortages =
                    shortages
                        .Where(s =>
                            s.WorkOrderNumber ==
                            wo.WorkOrderNumber)
                        .ToList();

                Debug.WriteLine(
                    $"{wo.WorkOrderNumber} shortages = {woShortages.Count}");

                if (woShortages.Any())
                {
                    readiness.MaterialsReady = false;

                    readiness.Reason =
                        $"{woShortages.First().ItemCode} short by " +
                        $"{woShortages.First().ShortageQuantity}";

                    Debug.WriteLine(
                        $"{wo.WorkOrderNumber} BLOCKED");
                }
                else
                {
                    Debug.WriteLine(
                        $"{wo.WorkOrderNumber} READY");
                }

                result.Add(readiness);
            }

            return result;
        }
    }
}
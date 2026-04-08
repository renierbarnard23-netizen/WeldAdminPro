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

            // ✅ TRUE BLOCKED (from shortages)
            int blocked = workOrders.Count(w =>
                blockedWorkOrders.Contains(w.WorkOrderNumber));

            // ✅ TRUE READY (status-based + not blocked)
            int ready = workOrders.Count(w =>
                w.Status == WorkOrderStatus.Ready &&
                !blockedWorkOrders.Contains(w.WorkOrderNumber));

            // ✅ OPTIONAL (for sanity)
            int inProduction = workOrders.Count(w => w.Status == WorkOrderStatus.InProduction);
            int completed = workOrders.Count(w => w.Status == WorkOrderStatus.Completed);

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
    }
}
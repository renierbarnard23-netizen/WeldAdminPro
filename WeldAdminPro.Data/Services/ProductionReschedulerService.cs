using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services
{
	public class ProductionReschedulerService
	{
		private readonly ProductionStatusService _statusService;
		private readonly ProductionSchedulingService _schedulingService;

		public ProductionReschedulerService()
		{
			_statusService = new ProductionStatusService();
			_schedulingService = new ProductionSchedulingService();
		}

		public void RecalculateProduction()
		{
			// 1 Update work order statuses
			_statusService.RefreshStatuses();

			// 2 Rebuild queue (priority recalculation happens inside scheduling)
			_schedulingService.BuildQueue();
		}
	}
}
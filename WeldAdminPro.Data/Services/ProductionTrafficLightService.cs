using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
	public class ProductionTrafficLightService
	{
		private readonly WorkOrderStatusService _statusService;

		public ProductionTrafficLightService()
		{
			_statusService = new WorkOrderStatusService();
		}

        public List<ProductionTrafficLightKpi> BuildKpis()
        {
            var statuses = _statusService.GetStatuses();

            var ready = statuses.Count(s => s.Status == WorkOrderExecutionStatus.Ready);
            var blocked = statuses.Count(s => s.Status == WorkOrderExecutionStatus.Blocked);
            var inProduction = statuses.Count(s => s.Status == WorkOrderExecutionStatus.InProduction);
            var completed = statuses.Count(s => s.Status == WorkOrderExecutionStatus.Completed);

            return new List<ProductionTrafficLightKpi>
    {
        new ProductionTrafficLightKpi
        {
            Title = "Ready Work Orders",
            Value = ready,
            Color = "Green"
        },
        new ProductionTrafficLightKpi
        {
            Title = "Blocked Work Orders",
            Value = blocked,
            Color = "Red"
        },
        new ProductionTrafficLightKpi
        {
            Title = "In Production",
            Value = inProduction,
            Color = "Orange"
        },
        new ProductionTrafficLightKpi
        {
            Title = "Completed",
            Value = completed,
            Color = "SteelBlue"
        }
    };
        }
    }
}
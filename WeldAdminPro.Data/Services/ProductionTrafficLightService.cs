using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

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

			var ready = statuses.Count(s => s.Status.ToString() == "Ready");
			var blocked = statuses.Count(s => s.Status.ToString() == "Blocked");
			var inProduction = statuses.Count(s => s.Status.ToString() == "InProduction");
			var completed = statuses.Count(s => s.Status.ToString() == "Completed");

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
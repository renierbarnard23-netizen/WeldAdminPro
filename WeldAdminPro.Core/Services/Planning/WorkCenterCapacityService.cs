using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Planning
{
	public class WorkCenterCapacityService
	{
		public List<WorkCenter> CalculateCapacity(IEnumerable<WorkOrder> workOrders)
		{
			var centers = new List<WorkCenter>
			{
				new WorkCenter { Name = "Welding", HoursPerDay = 16 },
				new WorkCenter { Name = "Assembly", HoursPerDay = 12 },
				new WorkCenter { Name = "QA", HoursPerDay = 8 }
			};

			foreach (var wo in workOrders)
			{
				var center = centers.FirstOrDefault(c => c.Name == wo.WorkCenter);

				if (center == null)
					continue;

				center.CurrentLoadHours += wo.EstimatedHours;
			}

			return centers;
		}
	}
}
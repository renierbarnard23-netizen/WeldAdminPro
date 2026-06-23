using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Production.Services
{
	public static class DependencyValidator
	{
		public static bool CanExecute(
			WorkOrder workOrder,
			IEnumerable<WorkOrder> allWorkOrders,
			out string reason)
		{
			reason = string.Empty;

			if (workOrder.DependencyIds == null || !workOrder.DependencyIds.Any())
				return true;

			var dependencies = allWorkOrders
				.Where(w => workOrder.DependencyIds.Contains(w.Id))
				.ToList();

			var incomplete = dependencies
				.Where(d => d.Status != WorkOrderStatus.Completed)
				.ToList();

			if (incomplete.Any())
			{
				reason = $"Blocked by {incomplete.Count} incomplete work order(s)";
				return false;
			}

			return true;
		}
	}
}
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services.Interfaces
{
	public interface IRiskService
	{
		List<WeldAdminPro.Core.Models.DeadlineRisk> GetDeadlineRisks(IEnumerable<WorkOrder> workOrders);
	}
}
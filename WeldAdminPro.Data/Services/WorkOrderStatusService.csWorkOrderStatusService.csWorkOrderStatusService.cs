using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class WorkOrderStatusService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly ProductionBlockService _blockService;

		public WorkOrderStatusService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_blockService = new ProductionBlockService();
		}

		public List<WorkOrderExecutionStatusModel> GetStatuses()
		{
			var workOrders = _workOrderRepository.GetAll();
			var blocked = _blockService.GetBlockedWorkOrders();

			var blockedNumbers = blocked
				.Select(b => b.WorkOrderNumber)
				.ToHashSet();

			var result = new List<WorkOrderExecutionStatusModel>();

			foreach (var wo in workOrders)
			{
				var status = WorkOrderExecutionStatus.Ready;
				var reason = "";

				if (wo.CompletedOn != null)
				{
					status = WorkOrderExecutionStatus.Completed;
				}
				else if (blockedNumbers.Contains(wo.WorkOrderNumber))
				{
					status = WorkOrderExecutionStatus.Blocked;
					reason = "Material shortage";
				}

				result.Add(new WorkOrderExecutionStatusModel
				{
					WorkOrderNumber = wo.WorkOrderNumber,
					Status = status,
					Reason = reason
				});
			}

			return result;
		}
	}
}
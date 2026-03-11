using System;
using System.Linq;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionStatusService
	{
		public void RefreshStatuses()
		{
			var woRepo = new WorkOrderRepository();

			var orders = woRepo.GetAll().ToList();

			foreach (var wo in orders)
			{
				// For now we only ensure completed jobs remain completed
				if (wo.Status == WorkOrderStatus.Completed)
					continue;

				woRepo.Update(wo);
			}
		}
	}
}
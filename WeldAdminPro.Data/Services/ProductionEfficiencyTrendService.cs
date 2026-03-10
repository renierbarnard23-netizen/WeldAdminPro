using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionEfficiencyTrendService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionEfficiencyTrendService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public List<ProductionEfficiencyTrendModel> GetLast7DaysTrend()
		{
			var orders = _workOrderRepository.GetAll().ToList();

			var today = DateTime.Today;

			var result = new List<ProductionEfficiencyTrendModel>();

			for (int i = 6; i >= 0; i--)
			{
				var day = today.AddDays(-i);

				var completed = orders
					.Where(o => o.Status == WorkOrderStatus.Completed)
					.Count();

				result.Add(new ProductionEfficiencyTrendModel
				{
					Date = day,
					CompletedWorkOrders = completed
				});
			}

			return result;
		}
	}
}
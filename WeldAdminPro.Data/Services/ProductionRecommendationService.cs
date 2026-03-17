using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionRecommendationService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionRecommendationService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public List<ProductionRecommendationModel> GetRecommendations()
		{
			var results = new List<ProductionRecommendationModel>();

			var orders = _workOrderRepository.GetAll()
				.Where(o => o.Status == WorkOrderStatus.Ready)
				.ToList();

			var today = DateTime.Today;

			foreach (var order in orders)
			{
				int score = 0;

				// Deadline urgency
				if (order.DueDate.HasValue)
				{
					var days = (order.DueDate.Value.Date - today).Days;

					if (days <= 1) score += 50;
					else if (days <= 3) score += 30;
					else if (days <= 7) score += 10;
				}

				// Priority bonus
				score += order.Priority * 5;

				results.Add(new ProductionRecommendationModel
				{
					WorkOrderNumber = order.WorkOrderNumber,
					Recommendation = "Consider starting this job next",
					Explanation = "AI production priority engine",
					Score = score
				});
			}

			return results
				.OrderByDescending(r => r.Score)
				.Take(5)
				.ToList();
		}
	}
}
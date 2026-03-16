using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionAIPlannerService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly WorkOrderShortageDetectionService _shortageService;

		public ProductionAIPlannerService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_shortageService = new WorkOrderShortageDetectionService();
		}

		public List<AIProductionRecommendation> GetRecommendations()
		{
			var workOrders = _workOrderRepository
				.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.ToList();

			var shortages = _shortageService
				.DetectShortages()
				.Select(s => s.WorkOrderNumber)
				.ToList();

			var recommendations = new List<AIProductionRecommendation>();

			foreach (var wo in workOrders)
			{
				bool materialsReady = !shortages.Contains(wo.WorkOrderNumber);

				double score = CalculatePriorityScore(wo, materialsReady);

				string recommendation;

				if (!materialsReady)
					recommendation = "Blocked – Resolve Material";
				else if (score >= 90)
					recommendation = "Start Immediately";
				else if (score >= 60)
					recommendation = "Queue Next";
				else
					recommendation = "Schedule Later";

				
				var daysUntilDue = wo.DueDate.HasValue
	? (wo.DueDate.Value - DateTime.Today).TotalDays
	: 30;

				string dueText =
					daysUntilDue <= 1 ? "Tomorrow" :
					daysUntilDue <= 7 ? "This Week" :
					daysUntilDue <= 14 ? "2 Weeks" :
					"Later";

				recommendations.Add(new AIProductionRecommendation
				{
					WorkOrderNumber = wo.WorkOrderNumber,
					Due = dueText,
					Materials = materialsReady ? "Ready" : "Shortage",
					PriorityScore = Math.Round(score),
					Recommendation = recommendation
				});
			}



			return recommendations
				.OrderByDescending(r => r.PriorityScore)
				.ToList();
		}

		private double CalculatePriorityScore(WorkOrder wo, bool materialsReady)
		{
			double score = 0;

			var daysUntilDue = wo.DueDate.HasValue
				? (wo.DueDate.Value - DateTime.Today).TotalDays
				: 30;

			if (daysUntilDue <= 1) score += 60;
			else if (daysUntilDue <= 3) score += 45;
			else if (daysUntilDue <= 7) score += 30;
			else if (daysUntilDue <= 14) score += 15;
			else score += 5;

			if (materialsReady)
				score += 15;

			if (wo.EstimatedHours <= 4)
				score += 20;
			else if (wo.EstimatedHours <= 8)
				score += 15;

			score += 5;

			return score;
		}

		public List<ProductionScheduleItem> GenerateOptimalSchedule()
		{
			var workOrders = _workOrderRepository
				.GetAll()
				.Where(w =>
				w.Status != WorkOrderStatus.Completed &&
				w.Status != WorkOrderStatus.InProduction)
				.OrderBy(w => w.DueDate)
				.ThenBy(w => w.EstimatedHours)
				.ToList();

			var result = new List<ProductionScheduleItem>();

			DateTime currentStart = DateTime.Today;

			foreach (var wo in workOrders)
			{
				double hours = wo.EstimatedHours <= 0 ? 8 : wo.EstimatedHours;
				double days = hours / 8.0;

				var schedule = new ProductionScheduleItem
				{
					WorkOrderNumber = wo.WorkOrderNumber,
					StartDate = currentStart,
					EndDate = currentStart.AddDays(days)
				};

				result.Add(schedule);

				currentStart = schedule.EndDate;
			}

			return result;
		}
	}
}
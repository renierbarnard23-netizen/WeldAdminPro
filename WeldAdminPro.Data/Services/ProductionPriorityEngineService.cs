using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionPriorityEngineService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly WorkOrderShortageDetectionService _shortageService;

		public ProductionPriorityEngineService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_shortageService = new WorkOrderShortageDetectionService();
		}

		public List<ProductionPriorityScore> CalculatePriority()
		{
			var workOrders = _workOrderRepository
				.GetAll()
				.Where(w => w.Status != WorkOrderStatus.Completed)
				.ToList();

			var shortages = _shortageService
				.DetectShortages()
				.Select(s => s.WorkOrderNumber)
				.Distinct()
				.ToList();

			var result = new List<ProductionPriorityScore>();

			foreach (var wo in workOrders)
			{
				bool materialsReady = !shortages.Contains(wo.WorkOrderNumber);

				double dueScore = CalculateDueDateScore(wo.DueDate);
				double materialScore = materialsReady ? 30 : 0;
				double durationScore = CalculateDurationScore(wo.EstimatedHours);

				double totalScore =
					dueScore +
					materialScore +
					durationScore;

				result.Add(new ProductionPriorityScore
				{
					WorkOrderNumber = wo.WorkOrderNumber,
					Score = totalScore,
					MaterialsReady = materialsReady,
					DueDateScore = dueScore,
					MaterialScore = materialScore,
					DurationScore = durationScore
				});
			}

			return result
				.OrderByDescending(p => p.Score)
				.ToList();
		}

		private double CalculateDueDateScore(DateTime? dueDate)
		{
			if (!dueDate.HasValue)
				return 0;

			var days = (dueDate.Value - DateTime.Today).TotalDays;

			if (days <= 1) return 40;
			if (days <= 3) return 30;
			if (days <= 7) return 20;

			return 10;
		}

		private double CalculateDurationScore(double hours)
		{
			if (hours <= 4) return 20;
			if (hours <= 8) return 15;
			if (hours <= 16) return 10;

			return 5;
		}
	}
}
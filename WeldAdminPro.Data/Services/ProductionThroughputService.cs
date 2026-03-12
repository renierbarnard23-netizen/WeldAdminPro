using System;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionThroughputService
	{
		private readonly WorkOrderRepository _workOrderRepository;

		public ProductionThroughputService()
		{
			_workOrderRepository = new WorkOrderRepository();
		}

		public ProductionThroughputModel GetThroughput()
		{
			var orders = _workOrderRepository.GetAll().ToList();

			var completed = orders
				.Where(o => o.Status == WorkOrderStatus.Completed)
				.ToList();

			// Completed today
			int completedToday = completed.Count(o =>
				o.CompletedOn.HasValue &&
				o.CompletedOn.Value.Date == DateTime.Today);

			// Active work orders
			int activeOrders = orders.Count(o =>
				o.Status == WorkOrderStatus.InProduction);

			// Completion rate
			double completionRate = orders.Count == 0
				? 0
				: Math.Round((double)completed.Count / orders.Count * 100, 1);

			// Duration calculation
			var durations = completed
	.Where(o => o.ActualStartTime != null && o.CompletedOn != null)
	.Select(o => (o.CompletedOn!.Value - o.ActualStartTime!.Value).TotalHours)
	.ToList();

			double avgDuration = durations.Any()
				? Math.Round(durations.Average(), 2)
				: 0;

			Console.WriteLine($"AVG DURATION: {avgDuration}");

			return new ProductionThroughputModel
			{
				CompletedToday = completedToday,
				AvgDurationHours = avgDuration,
				ActiveWorkOrders = activeOrders,
				CompletionRate = completionRate
			};
		}
	}
}
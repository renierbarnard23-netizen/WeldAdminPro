using System;
using System.Collections.Generic;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionSimulationService
	{
		public List<ProductionCompletionPrediction> SimulateSchedule(
			List<ProductionQueueItem> queue,
			double hoursPerDay = 8)
		{
			var results = new List<ProductionCompletionPrediction>();

			DateTime current = DateTime.Today;

			foreach (var job in queue)
			{
				var durationDays = job.EstimatedHours / hoursPerDay;

				var start = current;
				var end = start.AddDays(durationDays);

				bool isLate = end > job.Deadline;
				int daysLate = isLate ? (end - job.Deadline).Days : 0;

				results.Add(new ProductionCompletionPrediction
				{
					WorkOrderNumber = job.WorkOrderNumber,
					PredictedStart = start,
					PredictedEnd = end,
					IsLate = isLate,
					DaysLate = daysLate
				});

				current = end;
			}

			return results;
		}
	}
}
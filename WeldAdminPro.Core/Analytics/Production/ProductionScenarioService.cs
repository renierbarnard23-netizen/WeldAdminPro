using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionScenarioService
	{
		public ProductionScenarioResult SimulateScenario(
			List<ProductionQueueItem> queue,
			double hoursPerDay = 8)
		{
			var simulation = new ProductionSimulationService();

			var predictions = simulation.SimulateSchedule(queue, hoursPerDay);

			int lateJobs = predictions.Count(p => p.IsLate);
			int totalDelay = predictions.Sum(p => p.DaysLate);

			return new ProductionScenarioResult
			{
				Predictions = predictions,
				LateJobs = lateJobs,
				TotalDelayDays = totalDelay
			};
		}
	}
}
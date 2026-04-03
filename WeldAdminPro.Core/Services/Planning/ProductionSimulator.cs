using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Core.Services.Planning
{
	public class SimulationResult
	{
		public string WorkOrderNumber { get; set; } = "";

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public DateTime Deadline { get; set; }

		public bool IsLate => EndDate > Deadline;

		public int DelayDays => IsLate
			? (EndDate - Deadline).Days
			: 0;
	}

	public class ProductionSimulator
	{
		public List<SimulationResult> Simulate(
			List<ProductionQueueItem> queue,
			double hoursPerDay)
		{
			var results = new List<SimulationResult>();

			DateTime currentTime = DateTime.Today;

			foreach (var job in queue)
			{
				double jobHours = job.EstimatedHours;

				int daysRequired = (int)Math.Ceiling(jobHours / hoursPerDay);

				var start = currentTime;
				var end = start.AddDays(daysRequired);

				results.Add(new SimulationResult
				{
					WorkOrderNumber = job.WorkOrderNumber,
					StartDate = start,
					EndDate = end,
					Deadline = job.Deadline
				});

				currentTime = end;
			}

			return results;
		}
	}
}
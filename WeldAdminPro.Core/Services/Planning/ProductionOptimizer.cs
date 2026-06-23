using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Core.Services.Planning
{
	public class OptimizationResult
	{
		public List<ProductionQueueItem> BestSequence { get; set; } = new();

		public int TotalDelayDays { get; set; }
		public int LateJobs { get; set; }

		public string Explanation { get; set; } = "";
	}

	public class ProductionOptimizer
	{
		private readonly ProductionSimulator _simulator = new();

		public OptimizationResult Optimize(
			List<ProductionQueueItem> originalQueue,
			double hoursPerDay)
		{
			var scenarios = GenerateScenarios(originalQueue);

			OptimizationResult? best = null;
			OptimizationResult? secondBest = null;

			foreach (var scenario in scenarios)
			{
				var sim = _simulator.Simulate(scenario, hoursPerDay);

				var totalDelay = sim.Sum(r => r.DelayDays);
				var lateJobs = sim.Count(r => r.IsLate);

				var current = new OptimizationResult
				{
					BestSequence = scenario,
					TotalDelayDays = totalDelay,
					LateJobs = lateJobs
				};

				if (best == null ||
					totalDelay < best.TotalDelayDays ||
					(totalDelay == best.TotalDelayDays && lateJobs < best.LateJobs))
				{
					secondBest = best;
					best = current;
				}
				else if (secondBest == null ||
						 totalDelay < secondBest.TotalDelayDays)
				{
					secondBest = current;
				}
			}

			// ✅ IMPACT-BASED EXPLANATION
			if (best != null)
			{
				if (secondBest != null)
				{
					var improvement = secondBest.TotalDelayDays - best.TotalDelayDays;

					best.Explanation =
						improvement > 0
						? $"Choosing this sequence avoids {improvement} delay day(s) compared to alternatives."
						: $"All tested sequences result in the same delay ({best.TotalDelayDays} days).";
				}
				else
				{
					best.Explanation =
						$"Best sequence results in {best.TotalDelayDays} delay days.";
				}
			}

			return best!;
		}

		private List<List<ProductionQueueItem>> GenerateScenarios(List<ProductionQueueItem> queue)
		{
			var scenarios = new List<List<ProductionQueueItem>>();

			// 1. Original
			scenarios.Add(queue.ToList());

			// 2. Reverse
			scenarios.Add(queue.AsEnumerable().Reverse().ToList());

			// 3. Swap first two
			if (queue.Count >= 2)
			{
				var swapped = queue.ToList();
				(swapped[0], swapped[1]) = (swapped[1], swapped[0]);
				scenarios.Add(swapped);
			}

			// 4. Sort by earliest deadline
			scenarios.Add(queue.OrderBy(q => q.Deadline).ToList());

			// 5. Shortest processing time first
			scenarios.Add(queue.OrderBy(q => q.EstimatedHours).ToList());

			// 6. Longest processing time first
			scenarios.Add(queue.OrderByDescending(q => q.EstimatedHours).ToList());

			// 7. Random shuffle (AI exploration)
			var rnd = new Random();
			scenarios.Add(queue.OrderBy(q => rnd.Next()).ToList());

			return scenarios;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionOptimizerService
	{
		public ProductionScenarioResult FindBestSequence(List<ProductionQueueItem> originalQueue)
		{
			var scenarioService = new ProductionScenarioService();

			var scenarios = new List<(string Name, List<ProductionQueueItem> Queue)>();

			// 1️⃣ Current order
			scenarios.Add(("Current", originalQueue));

			// 2️⃣ Reverse order
			scenarios.Add(("Reversed", originalQueue.AsEnumerable().Reverse().ToList()));

			// 3️⃣ Priority-based (high first)
			scenarios.Add(("Priority High First", originalQueue.OrderByDescending(x => x.Priority).ToList()));

			// 4️⃣ Earliest deadline first
			scenarios.Add(("Earliest Deadline", originalQueue.OrderBy(x => x.Deadline).ToList()));

			ProductionScenarioResult? best = null;
			string bestName = "";

			foreach (var scenario in scenarios)
			{
				var result = scenarioService.SimulateScenario(scenario.Queue);

				System.Diagnostics.Debug.WriteLine($"Scenario: {scenario.Name}");
				System.Diagnostics.Debug.WriteLine($"Late Jobs: {result.LateJobs}, Delay: {result.TotalDelayDays}");

				if (best == null ||
					result.TotalDelayDays < best.TotalDelayDays ||
					(result.TotalDelayDays == best.TotalDelayDays &&
					 result.LateJobs < best.LateJobs))
				{
					best = result;
					bestName = scenario.Name;
				}
			}

			System.Diagnostics.Debug.WriteLine("=== BEST SCENARIO ===");
			System.Diagnostics.Debug.WriteLine(bestName);

			return best ?? new ProductionScenarioResult();
		}
	}
}
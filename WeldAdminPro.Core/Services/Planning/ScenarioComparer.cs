using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Core.Services.Planning
{
	public class ScenarioResult
	{
		public string Name { get; set; } = "";
		public List<SimulationResult> Results { get; set; } = new();

		public int TotalDelayDays =>
			Results.Sum(r => r.DelayDays);

		public int LateJobs =>
			Results.Count(r => r.IsLate);
	}

	public class ScenarioComparer
	{
		private readonly ProductionSimulator _simulator = new();

		public ScenarioResult Compare(
			string name,
			List<ProductionQueueItem> queue,
			double hoursPerDay)
		{
			var sim = _simulator.Simulate(queue, hoursPerDay);

			return new ScenarioResult
			{
				Name = name,
				Results = sim
			};
		}
	}
}
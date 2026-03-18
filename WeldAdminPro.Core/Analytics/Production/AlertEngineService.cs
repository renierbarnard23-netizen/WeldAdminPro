using WeldAdminPro.Core.Analytics.Production;

public class AlertEngineService
{
	public List<SystemAlert> GenerateAlerts(
		List<ProductionBottleneckModel> bottlenecks,
		ProductionControlSnapshot controlTower,
		IEnumerable<ProductionCapacityForecast> capacity)
	{
		var alerts = new List<SystemAlert>();

		// 🔴 HIGH SEVERITY BOTTLENECKS
		foreach (var b in bottlenecks.Where(x => x.Severity == "High"))
		{
			alerts.Add(new SystemAlert
			{
				Level = AlertLevel.Critical,
				Message = $"{b.WorkOrderNumber} blocked: {b.Description}",
				Source = "Production"
			});
		}

		// 🔴 CAPACITY OVERLOAD
		if (controlTower?.CapacityLoad >= 100)
		{
			alerts.Add(new SystemAlert
			{
				Level = AlertLevel.Critical,
				Message = "Production capacity overloaded",
				Source = "Capacity"
			});
		}

		// 🟠 NEAR CAPACITY
		else if (controlTower?.CapacityLoad >= 80)
		{
			alerts.Add(new SystemAlert
			{
				Level = AlertLevel.Warning,
				Message = "Production nearing capacity",
				Source = "Capacity"
			});
		}

		// 🟠 FUTURE OVERLOAD
		if (capacity != null && capacity.Any(c => c.LoadPercentage >= 100))
		{
			alerts.Add(new SystemAlert
			{
				Level = AlertLevel.Warning,
				Message = "Upcoming capacity overload detected",
				Source = "Forecast"
			});
		}

		return alerts;
	}
}
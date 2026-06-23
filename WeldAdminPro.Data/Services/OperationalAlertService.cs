using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;

namespace WeldAdminPro.Data.Services
{
	public class OperationalAlertService
	{
		private readonly MaterialDemandForecastService _forecastService;
		private readonly InventoryRiskSummaryService _riskService;
		private readonly InventoryAnomalyDetectionService _anomalyService;

		public OperationalAlertService()
		{
			_forecastService = new MaterialDemandForecastService();
			_riskService = new InventoryRiskSummaryService();
			_anomalyService = new InventoryAnomalyDetectionService();
		}

		public List<OperationalAlert> GenerateAlerts()
		{
			var alerts = new List<OperationalAlert>();

			var forecast = _forecastService.GenerateForecast();

			foreach (var item in forecast.Where(f => f.DaysRemaining <= 5))
			{
				alerts.Add(new OperationalAlert
				{
					Message = $"{item.ItemCode} will run out in {item.DaysRemaining} days",
					Severity = "Critical"
				});
			}

			var risk = _riskService.BuildSummary();

			if (risk.HealthScore < 70)
			{
				alerts.Add(new OperationalAlert
				{
					Message = $"Inventory health dropped to {risk.HealthScore}%",
					Severity = "Warning"
				});
			}

			var anomalies = _anomalyService.DetectAnomalies();

			foreach (var anomaly in anomalies.Take(3))
			{
				alerts.Add(new OperationalAlert
				{
					Message = $"{anomaly.ItemCode} consumption spike detected (+{(int)anomaly.IncreasePercentage}%)",
					Severity = anomaly.Severity
				});
			}

			return alerts;
		}
	}
}
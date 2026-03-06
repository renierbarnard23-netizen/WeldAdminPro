using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class HomeViewModel : ObservableObject
	{
		private readonly InventoryRiskSummaryService _riskSummaryService;
		private readonly MaterialDemandForecastService _forecastService;

		public HomeViewModel()
		{
			_riskSummaryService = new InventoryRiskSummaryService();
			_forecastService = new MaterialDemandForecastService();

			LoadRiskSummary();
			LoadProcurementAlerts();
		}

		// =========================================================
		// EXECUTIVE RISK INDICATORS
		// =========================================================

		[ObservableProperty]
		private string inventoryRiskLevel = "LOW";

		[ObservableProperty]
		private int negativeStockItems;

		[ObservableProperty]
		private int deadStockItems;

		[ObservableProperty]
		private int consumptionSpikes;

		[ObservableProperty]
		private int criticalInventoryItems;

		[ObservableProperty]
		private int inventoryHealthScore;

		[ObservableProperty]
		private ObservableCollection<MaterialDemandForecast> procurementAlerts = new();

		private void LoadRiskSummary()
		{
			var summary = _riskSummaryService.BuildSummary();

			InventoryRiskLevel = summary.OverallRiskLevel;
			NegativeStockItems = summary.NegativeStockItems;
			DeadStockItems = summary.DeadStockItems;
			ConsumptionSpikes = summary.ConsumptionSpikes;
			CriticalInventoryItems = summary.CriticalInventoryItems;
			InventoryHealthScore = summary.HealthScore;
		}
		private void LoadProcurementAlerts()
		{
			var forecast = _forecastService.GenerateForecast();

			var urgentItems = forecast
				.Where(f => f.PriorityScore >= 50)
				.Take(10);

			ProcurementAlerts = new ObservableCollection<MaterialDemandForecast>(urgentItems);
		}
	}
}
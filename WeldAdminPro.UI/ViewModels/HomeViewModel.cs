using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Analytics.Procurement;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class HomeViewModel : ObservableObject
	{
		private readonly InventoryRiskSummaryService _riskSummaryService;
		private readonly MaterialDemandForecastService _forecastService;
		private readonly MaterialConsumptionService _consumptionService;
		private readonly InventoryAnomalyDetectionService _anomalyService;
		private readonly OperationalAlertService _alertService;
		private readonly ProjectProfitabilityIntelligenceService _profitService;
		private readonly ExecutiveKpiService _kpiService;
		private readonly WorkOrderMaterialPlanningService _planningService;
		private readonly WorkOrderShortageDetectionService _shortageService;
		private readonly ProcurementSuggestionService _procurementService;

		public ObservableCollection<WorkOrderMaterialShortage> MaterialShortages { get; set; } = new();

		public HomeViewModel()
		{
			_riskSummaryService = new InventoryRiskSummaryService();
			_forecastService = new MaterialDemandForecastService();
			_consumptionService = new MaterialConsumptionService();
			_anomalyService = new InventoryAnomalyDetectionService();
			_alertService = new OperationalAlertService();
			_profitService = new ProjectProfitabilityIntelligenceService();
			_kpiService = new ExecutiveKpiService();
			_planningService = new WorkOrderMaterialPlanningService();
			_shortageService = new WorkOrderShortageDetectionService();
			_procurementService = new ProcurementSuggestionService();

			LoadRiskSummary();
			LoadProcurementAlerts();
			LoadConsumptionStats();
			LoadOperationalAlerts();
			LoadProjectProfitability();
			LoadKpis();
			LoadWorkOrderPlan();
			LoadMaterialShortages();
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

		[ObservableProperty]
		private ObservableCollection<MaterialConsumptionStat> topConsumedMaterials = new();

		[ObservableProperty]
		private ObservableCollection<InventoryAnomaly> inventoryAnomalies = new();

		[ObservableProperty]
		private ObservableCollection<OperationalAlert> operationalAlerts = new();

		[ObservableProperty]
		private ObservableCollection<ProjectProfitabilityStat> projectProfitability = new();

		[ObservableProperty]
		private ObservableCollection<ExecutiveKpi> executiveKpis = new();

		[ObservableProperty]
		private ObservableCollection<WorkOrderMaterialPlan> workOrderMaterialPlans = new();

		[ObservableProperty]
		private ObservableCollection<ProcurementSuggestion> procurementSuggestions = new();

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

		private void LoadConsumptionStats()
		{
			var stats = _consumptionService.GetTopConsumed();

			TopConsumedMaterials = new ObservableCollection<MaterialConsumptionStat>(stats);
		}

		private void LoadAnomalies()
		{
			var anomalies = _anomalyService.DetectAnomalies();

			InventoryAnomalies = new ObservableCollection<InventoryAnomaly>(anomalies);
		}

		private void LoadOperationalAlerts()
		{
			var alerts = _alertService.GenerateAlerts();

			OperationalAlerts = new ObservableCollection<OperationalAlert>(alerts);
		}

		private void LoadProjectProfitability()
		{
			var stats = _profitService.GetProjectProfitability();

			ProjectProfitability =
				new ObservableCollection<ProjectProfitabilityStat>(stats);
		}

		private void LoadKpis()
		{
			var kpis = _kpiService.BuildKpis(InventoryHealthScore);

			ExecutiveKpis = new ObservableCollection<ExecutiveKpi>(kpis);
		}

		private void LoadWorkOrderPlan()
		{
			var plan = _planningService.BuildPlan();

			WorkOrderMaterialPlans =
				new ObservableCollection<WorkOrderMaterialPlan>(plan);
		}

		private void LoadMaterialShortages()
		{
			var shortages = _shortageService.DetectShortages();

			MaterialShortages =
				new ObservableCollection<WorkOrderMaterialShortage>(shortages);
		}
		private void LoadProcurementSuggestions()
		{
			var suggestions = _procurementService.GenerateSuggestions();

			ProcurementSuggestions =
				new ObservableCollection<ProcurementSuggestion>(suggestions);
		}
	}
}
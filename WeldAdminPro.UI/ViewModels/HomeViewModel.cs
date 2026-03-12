using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Analytics.Procurement;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.ViewModels.Dashboard;

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
		private readonly ProductionReadinessService _productionReadinessService;
		private readonly ProductionBlockService _productionBlockService;
		private readonly WorkOrderStatusService _workOrderStatusService;
		private readonly ProductionTrafficLightService _trafficLightService;
		private readonly ProductionSchedulingService _schedulingService;
		private readonly MaterialReservationService _reservationService;
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly WorkOrderExecutionService _executionService;
		private readonly ProductionBottleneckDetectionService _bottleneckService;
		private readonly ProductionRecommendationService _recommendationService;
		private readonly ProductionThroughputService _throughputService;
		private readonly ProductionEfficiencyTrendService _efficiencyTrendService;

		public ObservableCollection<WorkOrderMaterialShortage> MaterialShortages { get; set; } = new();
		public ObservableCollection<ProductionGanttItem> ProductionTimeline { get; set; } = new();
		public ObservableCollection<ProductionCapacityForecast> CapacityForecast { get; set; } = new();

		public ObservableCollection<DeadlineRisk> DeadlineRisks { get; set; } = new();
		public ProductionControlViewModel Production { get; } = new ProductionControlViewModel();

		public ProductionExecutionViewModel Execution { get; }

		public ProductionControlTowerViewModel ProductionControlTower { get; } = new ProductionControlTowerViewModel();

		public List<ProductionBottleneckModel> ProductionBottlenecks { get; set; }
		public List<ProductionRecommendationModel> ProductionRecommendations { get; set; }
		public ProductionThroughputModel ProductionThroughput { get; set; }
		public List<ProductionEfficiencyTrendModel> ProductionEfficiencyTrend { get; set; }
		public ObservableCollection<SchedulerDebugItem> SchedulerDebug { get; set; }
	= new();
		public ObservableCollection<ProductionDelayPrediction> DelayPredictions { get; set; }
	= new();

		public event Action? ProductionChanged;


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
			_productionBlockService = new ProductionBlockService();
			_productionReadinessService = new ProductionReadinessService(
					new WorkOrderRepository(),
					new WorkOrderShortageDetectionService());
			_workOrderStatusService = new WorkOrderStatusService();
			_trafficLightService = new ProductionTrafficLightService();
			_schedulingService = new ProductionSchedulingService();
			_reservationService = new MaterialReservationService();
			_workOrderRepository = new WorkOrderRepository();
			_executionService = new WorkOrderExecutionService(_workOrderRepository);
			ProductionControlTower = new ProductionControlTowerViewModel();
			ProductionControlTower.Load();

			Execution = new ProductionExecutionViewModel();
			Execution.ControlTower = ProductionControlTower;
			Execution.Load();

			ProductionControlTower.Load();
			_bottleneckService = new ProductionBottleneckDetectionService();
			ProductionBottlenecks = _bottleneckService.DetectBottlenecks();
			_recommendationService = new ProductionRecommendationService();
			ProductionRecommendations = _recommendationService.GetRecommendations();
			_throughputService = new ProductionThroughputService();
			ProductionThroughput = _throughputService.GetThroughput();
			_efficiencyTrendService = new ProductionEfficiencyTrendService();
			ProductionEfficiencyTrend = _efficiencyTrendService.GetLast7DaysTrend();



			var workOrderRepo = _workOrderRepository;

			var capacityService = new ProductionCapacityService(workOrderRepo);

			var riskService = new DeadlineRiskDetectionService(
				workOrderRepo,
				capacityService);

			DeadlineRisks =
				new ObservableCollection<DeadlineRisk>(
					riskService.DetectRisks());

			CapacityForecast =
				new ObservableCollection<ProductionCapacityForecast>(
					capacityService.GetCapacityForecast());

			var ganttService = new ProductionGanttService(workOrderRepo);

			ProductionTimeline =
				new ObservableCollection<ProductionGanttItem>(
					ganttService.GetTimeline());
			var scheduler = new ProductionScheduleService();

			var schedule = scheduler.GetSchedule();

			SchedulerDebug.Clear();

			foreach (var s in schedule)
			{
				SchedulerDebug.Add(new SchedulerDebugItem
				{
					WorkOrderNumber = s.WorkOrderNumber,
					StartDate = s.StartDate,
					EndDate = s.EndDate,
					Hours = (s.EndDate - s.StartDate).TotalHours
				});
			}
			var delayService = new ProductionDelayPredictionService();

			var delays = delayService.PredictDelays();

			DelayPredictions.Clear();

			foreach (var d in delays)
			{
				DelayPredictions.Add(d);
			}


			LoadRiskSummary();
			LoadProcurementAlerts();
			LoadConsumptionStats();
			LoadOperationalAlerts();
			LoadProjectProfitability();
			LoadKpis();
			LoadWorkOrderPlan();
			LoadMaterialShortages();
			LoadProductionReadiness();
			InProduction = Execution.RunningWorkOrders.Count;
			Completed = Execution.CompletedToday.Count;
			LoadProcurementSuggestions();
			LoadProductionBlocks();
			LoadWorkOrderStatuses();
			LoadProductionTrafficLights();
			LoadProductionQueue();
			LoadReservations();
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

		[ObservableProperty]
		private double productionReadinessPercentage;

		[ObservableProperty]
		private int readyWorkOrders;

		[ObservableProperty]
		private int blockedWorkOrders;

		[ObservableProperty]
		private int inProduction;

		[ObservableProperty]
		private int completed;

		[ObservableProperty]
		private ObservableCollection<string> blockedWorkOrderNumbers = new();

		[ObservableProperty]
		private ObservableCollection<ProductionBlock> productionBlocks = new();

		[ObservableProperty]
		private ObservableCollection<ProductionQueueItem> productionQueue = new();

		[ObservableProperty]
		private ObservableCollection<WorkOrderExecutionStatusModel> workOrderStatuses = new();

		[ObservableProperty]
		private ObservableCollection<ProductionTrafficLightKpi> productionTrafficLights = new();

		[ObservableProperty]
		private ObservableCollection<MaterialReservation> materialReservations = new();

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
		private void LoadProductionReadiness()
		{
			var readiness = _productionReadinessService.Calculate();

			ProductionReadinessPercentage = readiness.ReadinessPercentage;
			ReadyWorkOrders = readiness.ReadyWorkOrders;
			BlockedWorkOrders = readiness.BlockedWorkOrders;

			BlockedWorkOrderNumbers =
				new ObservableCollection<string>(readiness.BlockedWorkOrderNumbers);

			InProduction = Execution.RunningWorkOrders.Count;
			Completed = Execution.CompletedToday.Count;
		}
		private void LoadProductionBlocks()
		{
			var blocks = _productionBlockService.GetBlockedWorkOrders();

			ProductionBlocks =
				new ObservableCollection<ProductionBlock>(blocks);
		}
		private void LoadWorkOrderStatuses()
		{
			var statuses = _workOrderStatusService.GetStatuses();

			WorkOrderStatuses =
				new ObservableCollection<WorkOrderExecutionStatusModel>(statuses);
		}
		private void LoadProductionTrafficLights()
		{
			var lights = _trafficLightService.BuildKpis();

			ProductionTrafficLights =
				new ObservableCollection<ProductionTrafficLightKpi>(lights);
		}
		private void LoadProductionQueue()
		{
			var queue = _schedulingService.BuildQueue();

			ProductionQueue =
				new ObservableCollection<ProductionQueueItem>(queue);
		}
		private void LoadReservations()
		{
			var reservations = _reservationService.GenerateReservations();

			MaterialReservations =
				new ObservableCollection<MaterialReservation>(reservations);
		}
		public void RefreshProductionSystem()
		{
			LoadMaterialShortages();
			LoadProductionBlocks();
			LoadProductionReadiness();
			LoadProductionQueue();
			LoadProductionTrafficLights();

			Execution.Load();
			ProductionControlTower.Load();
		}

		[RelayCommand]
		private void StartWorkOrder(Guid id)
		{
			_executionService.StartWorkOrder(id);

			RefreshProductionSystem();

			ProductionChanged?.Invoke();
		}

		[RelayCommand]
		private void PauseWorkOrder(Guid id)
		{
			_executionService.PauseWorkOrder(id);

			RefreshProductionSystem();

			ProductionChanged?.Invoke();
		}

		[RelayCommand]
		private void CompleteWorkOrder(Guid id)
		{
			_executionService.CompleteWorkOrder(id);

			RefreshProductionSystem();

			ProductionChanged?.Invoke();
		}


	}
	}

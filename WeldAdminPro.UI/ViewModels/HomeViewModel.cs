using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.VisualBasic;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Analytics.Procurement;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services.Interfaces;
using WeldAdminPro.Core.Services.Planning;
using WeldAdminPro.Core.Services.Risk;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.ViewModels.Dashboard;
using static QuestPDF.Helpers.Colors;

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
		
		private readonly ProductionBottleneckDetectionService _bottleneckService;

		private readonly ProductionThroughputService _throughputService;
		private readonly ProductionEfficiencyTrendService _efficiencyTrendService;

		private readonly ProductionReplanningService _replanningService = new();
		private readonly ProductionReplanTriggerService _replanTrigger = new();

		private readonly GlobalRiskService _globalRiskService;

		private readonly WorkOrderMaterialRepository _materialRepo;
		private readonly StockRepository _stockRepo;
		private List<StockItem> _allStock = new();

		private readonly WorkCenterCapacityService _capacityEngine = new();

		private bool _isRefreshing = false;

		public ObservableCollection<WorkOrderMaterialShortage> MaterialShortages { get; set; } = new();
		public ObservableCollection<ProductionGanttItem> ProductionTimeline { get; set; } = new();
		public ObservableCollection<ProductionCapacityForecast> CapacityForecast { get; set; } = new();

		public ObservableCollection<WeldAdminPro.Core.Models.DeadlineRisk> DeadlineRisks { get; set; } = new();
		public ProductionControlTowerViewModel ProductionControlTower { get; }
		public ProductionControlViewModel Production { get; } = new ProductionControlViewModel();

		public ProductionExecutionViewModel Execution { get; }
		public ProductionExecutionViewModel ProductionExecution { get; set; }

		public List<ProductionBottleneckModel> ProductionBottlenecks { get; set; }
		public ObservableCollection<ProductionRecommendationModel> ProductionRecommendations { get; set; } = new();
		public ObservableCollection<SchedulerDebugItem> SchedulerDebug { get; set; } = new();
		public ObservableCollection<ProductionDelayPrediction> DelayPredictions { get; set; } = new();
		public ProductionQueueItem? TopPriorityWorkOrder { get; set; }

		public event Action? ProductionChanged;
		public ProductionPlannerViewModel Planner { get; } = new ProductionPlannerViewModel();
		public ProductionAdvisorResult? AdvisorResult { get; set; }
		public ObservableCollection<SystemAlert> SystemAlerts { get; set; } = new();
		public ObservableCollection<ProductionCompletionPrediction> CompletionPredictions { get; set; } = new();
		public ObservableCollection<ProductionCompletionPrediction> ScenarioPredictions { get; set; } = new();
		public ObservableCollection<WorkOrderMaterialTrace> SelectedWorkOrderMaterials
	=> Execution.SelectedWorkOrderMaterials;

		public ObservableCollection<WorkCenter> WorkCenters { get; set; } = new();
		public int ScenarioLateJobs { get; set; }
		public int ScenarioTotalDelay { get; set; }
		public string OptimizedStrategy { get; set; } = "";
		public ICommand CancelWorkOrderCommand => Execution.CancelWorkOrderCommand;


		public HomeViewModel()
		{
			ProductionControlTower = new ProductionControlTowerViewModel();
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

			_globalRiskService = new GlobalRiskService();

			_materialRepo = new WorkOrderMaterialRepository();
			_stockRepo = new StockRepository();
			_allStock = _stockRepo.GetAll().ToList();

			// =========================
			// 🔥 FIXED EXECUTION SETUP
			// =========================

			var workOrderRepo = new WorkOrderRepository();

			var materialValidator = new MaterialValidator(
				_stockRepo,
				_materialRepo
			);

			var executionService = new WorkOrderExecutionService(
				workOrderRepo,
				_materialRepo,
				materialValidator,
				_stockRepo
			);

			// ✅ PASS INTO VIEWMODEL
			Execution = new ProductionExecutionViewModel(executionService);
			ProductionExecution = Execution;

			Execution.RefreshRequested = RefreshProductionSystem;

			ProductionControlTower.Load(DeadlineRisks.Count);

			Execution.ControlTower = ProductionControlTower;
			Execution.Load();

			// =========================
			// REMAINDER UNCHANGED
			// =========================

			_bottleneckService = new ProductionBottleneckDetectionService();
			ProductionBottlenecks = _bottleneckService.DetectBottlenecks();

			_throughputService = new ProductionThroughputService();
			ProductionThroughput = _throughputService.GetThroughput();

			_efficiencyTrendService = new ProductionEfficiencyTrendService();
			ProductionEfficiencyTrend = _efficiencyTrendService.GetLast7DaysTrend();

			LoadProductionQueue();


			// ===============================
			// 🔥 AI + OPTIMIZATION ENGINE
			// ===============================

			var comparer = new ScenarioComparer();
			var simulator = new ProductionSimulator();
			var optimizer = new ProductionOptimizer();

			var queueList = Production.ProductionQueue.ToList();

			// Scenario comparison
			var current = comparer.Compare("Current", queueList, 8);
			var reversed = comparer.Compare("Reversed", queueList.AsEnumerable().Reverse().ToList(), 8);

			System.Diagnostics.Debug.WriteLine($"Current Delay: {current.TotalDelayDays}");
			System.Diagnostics.Debug.WriteLine($"Reversed Delay: {reversed.TotalDelayDays}");

			// 🔥 OPTIMIZATION
			var autoOptimizer = new ProductionOptimizerService();

			var best = autoOptimizer.FindBestSequence(
				Production.ProductionQueue.ToList()
			);

			// Apply BEST sequence to UI
			Production.ProductionQueue = new ObservableCollection<ProductionQueueItem>(
				best.Predictions.Select(p =>
					Production.ProductionQueue.First(q => q.WorkOrderNumber == p.WorkOrderNumber)
				)
			);

			// Update scenario display
			ScenarioPredictions = new ObservableCollection<ProductionCompletionPrediction>(best.Predictions);
			ScenarioLateJobs = best.LateJobs;
			ScenarioTotalDelay = best.TotalDelayDays;

			OnPropertyChanged(nameof(ScenarioPredictions));
			OnPropertyChanged(nameof(ScenarioLateJobs));
			OnPropertyChanged(nameof(ScenarioTotalDelay));

			var result = optimizer.Optimize(queueList, 8);
			var topJob = result.BestSequence.FirstOrDefault();

			if (topJob != null)
			{
				AdvisorResult = new ProductionAdvisorResult
				{
					Recommendation = $"Start {topJob.WorkOrderNumber}",
					Explanation = result.Explanation
				};
			}

			System.Diagnostics.Debug.WriteLine("=== OPTIMIZED PLAN ===");

			foreach (var job in result.BestSequence)
			{
				System.Diagnostics.Debug.WriteLine(job.WorkOrderNumber);
			}

			System.Diagnostics.Debug.WriteLine($"Total Delay: {result.TotalDelayDays}");
			System.Diagnostics.Debug.WriteLine($"Late Jobs: {result.LateJobs}");

			// 🔥 SIMULATION (for debugging)
			var simResults = simulator.Simulate(queueList, 8);

			foreach (var r in simResults)
			{
				System.Diagnostics.Debug.WriteLine($"{r.WorkOrderNumber} | {r.StartDate:d} → {r.EndDate:d} | Late: {r.IsLate}");
			}

			// ===============================
			// 🔥 AI PRODUCTION PLANNER
			// ===============================

			var planner = new ProductionPlannerService();

			// ✅ MUST BE BEFORE planner
			var allWorkOrders = _workOrderRepository.GetAll().ToList();

			var plannerResult = planner.GeneratePlan(new PlanningContext
			{
				WorkOrders = allWorkOrders,

				GetMaterials = id => _materialRepo.GetByWorkOrderId(id)
							.Select(m => new MaterialRequirement
			{
							ItemCode = m.ItemCode,
							RequiredQuantity = m.RequiredQuantity
			}),

				GetStock = code => _allStock
					.FirstOrDefault(s => s.ItemCode == code)?.Quantity ?? 0,

				GetCapacity = wc => 8
			});

			// ===============================
			// 🔥 EXISTING SYSTEM (UNCHANGED)
			// ===============================

			var alertService = new AlertEngineService();

			var snapshot = new ProductionControlSnapshot
			{
				CapacityLoad = ProductionControlTower.CapacityLoad
			};

			var alerts = alertService.GenerateAlerts(
				ProductionBottlenecks,
				snapshot,
				CapacityForecast);

			SystemAlerts.Clear();
			foreach (var alert in alerts)
				SystemAlerts.Add(alert);

			var aiPlanner = new ProductionAIPlannerService();
			var aiRecommendations = aiPlanner.GetRecommendations();

			ProductionRecommendations = new ObservableCollection<ProductionRecommendationModel>(
				aiRecommendations.Select(r => new ProductionRecommendationModel
				{
					WorkOrderNumber = r.WorkOrderNumber,
					Recommendation = r.Recommendation,
					Explanation = r.Explanation,
					Score = (int)r.PriorityScore
				}));

			var capacityService = new ProductionCapacityService(_workOrderRepository);

			CapacityForecast = new ObservableCollection<ProductionCapacityForecast>(
				capacityService.GetCapacityForecast());

			var ganttService = new ProductionGanttService(_workOrderRepository);

			ProductionTimeline = new ObservableCollection<ProductionGanttItem>(
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
					Hours = (int)Math.Round((s.EndDate - s.StartDate).TotalHours)
				});
			}

			var delayService = new ProductionDelayPredictionService();
			var delays = delayService.PredictDelays();

			DelayPredictions.Clear();
			foreach (var d in delays)
				DelayPredictions.Add(d);

			// 🔥 BLOCK REASON ENGINE

			var engine = new BlockReasonEngine();

			var realWorkOrders = _workOrderRepository.GetAll().ToList();

			foreach (var order in realWorkOrders)
			{
				var blockResult = engine.Evaluate(order);

				order.BlockReason = blockResult.Reason;
				order.BlockMessage = blockResult.Message;
			}

			// ===============================
			// 🔥 FINAL LOADS
			// ===============================

			LoadRiskSummary();
			LoadProcurementAlerts();
			LoadConsumptionStats();
			LoadOperationalAlerts();
			LoadProjectProfitability();
			LoadKpis();
			LoadWorkOrderPlan();
			LoadMaterialShortages();
			LoadProductionReadiness();

			Func<Guid, IEnumerable<MaterialRequirement>> getMaterials = (woId) =>
			{
					return _materialRepo.GetByWorkOrderId(woId)
					.Select(m => new MaterialRequirement
					{
						ItemCode = m.ItemCode,
						RequiredQuantity = m.RequiredQuantity
					});
			};

			Func<string, double> getStock = (code) =>
			{
				var stock =_allStock.FirstOrDefault(s => s.ItemCode == code);
				return stock?.Quantity ?? 0;
			};

			LoadProductionBlocks(); // MUST be before risks

			var risks = _globalRiskService.GetAllRisks(
				allWorkOrders,
				getMaterials,
				getStock
			);

			ProductionRisks = new ObservableCollection<ProductionRisk>(risks);
			OnPropertyChanged(nameof(ProductionRisks));

			InProduction = allWorkOrders.Count(w => w.Status == WorkOrderStatus.InProduction);
			
			LoadProcurementSuggestions();
			LoadProductionBlocks();
			LoadWorkOrderStatuses();
			LoadProductionTrafficLights();
			LoadReservations();

			Planner.Load();

			RefreshProductionSystem();

			/// 🚫 AUTO REFRESH DISABLED (Phase 7 paused)
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

		[ObservableProperty]
		private ProductionThroughputModel productionThroughput = new();

		[ObservableProperty]
		private List<ProductionEfficiencyTrendModel> productionEfficiencyTrend = new();
		public ObservableCollection<ProductionRisk> ProductionRisks { get; set; } = new();
		private void LoadProductionTrafficLights()
		{
			var lights = _trafficLightService.BuildKpis();

			ProductionTrafficLights =
				new ObservableCollection<ProductionTrafficLightKpi>(lights);
		}
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

			var allWorkOrders = _workOrderRepository.GetAll();

			InProduction = allWorkOrders.Count(w => w.Status == WorkOrderStatus.InProduction);
			Completed = allWorkOrders.Count(w => w.Status == WorkOrderStatus.Completed);
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
		private void LoadProductionQueue()
		{
			
			var realWorkOrders = _workOrderRepository.GetAll().ToList();

			var queue = _schedulingService.BuildQueue();

			var engine = new BlockReasonEngine();
			
			foreach (var item in queue)
			{
				var wo = _workOrderRepository.GetAll()
					.FirstOrDefault(w => w.WorkOrderNumber == item.WorkOrderNumber);

				if (wo == null)
					continue;

				var materials = _materialRepo.GetByWorkOrderId(wo.Id);

				wo.MaterialRequirements = materials.Select(m =>
				{
					var stock = _allStock.FirstOrDefault(s => s.Id == m.ItemId);

					if (stock == null)
						stock = _allStock.FirstOrDefault(s => s.ItemCode == m.ItemCode);

					return new MaterialRequirement
					{
						ItemCode = m.ItemCode,
						RequiredQuantity = m.RequiredQuantity,
					};
				}).ToList();

				var result = engine.Evaluate(wo);

				item.BlockReason = result.Reason;
				item.BlockMessage = result.Message;
			}

			var autoPriority = new AutoPriorityService();

			var reordered = autoPriority.ReorderQueue(
				queue.ToList(),
				ProductionBottlenecks.ToList()
			);

			foreach (var q in reordered)
			{
				var real = realWorkOrders.FirstOrDefault(r => r.WorkOrderNumber == q.WorkOrderNumber);

				if (real != null)
				{
					Debug.WriteLine($"QUEUE STATUS: {real.Status}");

					q.Status = real.Status switch
					{
						WorkOrderStatus.Ready => "Ready",
						WorkOrderStatus.InProduction => "InProduction",
						WorkOrderStatus.Completed => "Completed",
						WorkOrderStatus.Paused => "Paused",
						_ => "Unknown"
					}; // ✅ correct
				}
			}

			var simulation = new ProductionSimulationService();

			var predictions = simulation.SimulateSchedule(
				reordered.ToList()
			);

			CompletionPredictions = new ObservableCollection<ProductionCompletionPrediction>(predictions);

			// 🔥 APPLY AI SCORING
			var scoring = new ProductionPriorityScoringService();
			reordered = scoring.Score(reordered, ProductionBottlenecks.ToList());

			// Reset flags
			foreach (var wo in reordered)
				wo.IsTopPriority = false;

			// 🔥 APPLY REPLANNING (THIS IS THE NEW ENGINE)
			var replanned = _replanningService.Replan(reordered);

			// 🔥 SET TOP PRIORITY
			var top = replanned.FirstOrDefault();

			if (top != null)
			{
				top.IsTopPriority = true;
				TopPriorityWorkOrder = top;
			}

			// 🔥 APPLY TO UI
			Production.ProductionQueue = new ObservableCollection<ProductionQueueItem>(replanned);

			OnPropertyChanged(nameof(Production));
			OnPropertyChanged(nameof(Production.ProductionQueue));
		}
		private void LoadReservations()
		{
			var reservations = _reservationService.GenerateReservations();

			MaterialReservations =
				new ObservableCollection<MaterialReservation>(reservations);
		}
		public void RefreshProductionSystem()
		{
			if (_isRefreshing)
			{
				Debug.WriteLine("⚠ REFRESH BLOCKED");
				return;
			}

			_isRefreshing = true;

			try
			{
				LoadMaterialShortages();
				LoadProductionBlocks();

				var allWorkOrders = _workOrderRepository.GetAll();

				Func<Guid, IEnumerable<MaterialRequirement>> getMaterials = (woId) =>
				{
					return _materialRepo.GetByWorkOrderId(woId)
						.Select(m => new MaterialRequirement
						{
							ItemCode = m.ItemCode,
							RequiredQuantity = m.RequiredQuantity
						});
				};

				Func<string, double> getStock = (code) =>
				{
					var stock = _allStock.FirstOrDefault(s => s.ItemCode == code);
					return stock?.Quantity ?? 0;
				};

				var risks = _globalRiskService.GetAllRisks(
					allWorkOrders,
					getMaterials,
					getStock
				);

				ProductionRisks = new ObservableCollection<ProductionRisk>(risks);
				OnPropertyChanged(nameof(ProductionRisks));

				LoadProductionReadiness();
				LoadProductionQueue();

			var runningJobs = Execution.RunningWorkOrders.ToList();

				foreach (var running in runningJobs)
				{
					bool isBlocked = ProductionBlocks.Any(b => b.WorkOrderNumber == running.WorkOrderNumber);

					if (isBlocked)
					{
						Debug.WriteLine($"[AUTO] STOPPING BLOCKED JOB: {running.WorkOrderNumber}");
						Execution.PauseWorkOrderCommand.Execute(running.Id);
					}
				}

				LoadProductionBlocks();   // 🔥 MUST be before risks

		OnPropertyChanged(nameof(ProductionRisks));

				Execution.Load();

				Debug.WriteLine($"FINAL RISKS BEFORE SET: {ProductionRisks.Count}");


				Debug.WriteLine($"VM INSTANCE: {ProductionControlTower.GetHashCode()}");
				

				// 🔥 FORCE UI UPDATE
				ProductionControlTower.Load(
				ProductionRisks.Count(r => r.RiskType == RiskType.Deadline)
				);

				// analytics
				ProductionThroughput = _throughputService.GetThroughput();
				ProductionEfficiencyTrend = _efficiencyTrendService.GetLast7DaysTrend();

				// AI + replanning (unchanged)

				LoadProductionTrafficLights(); // ✅ LAST

				ProductionThroughput = _throughputService.GetThroughput();
				ProductionEfficiencyTrend = _efficiencyTrendService.GetLast7DaysTrend();

				OnPropertyChanged(nameof(ProductionThroughput));
				OnPropertyChanged(nameof(ProductionEfficiencyTrend));

				// 🔥 AI OPTIMIZATION (RESTORED)
				var optimizer = new ProductionOptimizer();

				var optimized = optimizer.Optimize(
					Production.ProductionQueue.ToList(),
					8
				);

				var topJob = optimized.BestSequence.FirstOrDefault();

				if (topJob != null)
				{
					AdvisorResult = new ProductionAdvisorResult
					{
						Recommendation = $"Start {topJob.WorkOrderNumber}",
						Explanation = optimized.Explanation
					};
				}

				// 🔥 SMART REPLANNING (RESTORED)
				var currentQueue = Production.ProductionQueue.ToList();

				bool shouldReplan = ProductionRisks.Any() || ProductionBlocks.Any();

				if (shouldReplan)
				{
					var replanned = _replanningService.Replan(currentQueue);
					RunScenarioSimulation(replanned.ToList());

					Production.ProductionQueue =
						new ObservableCollection<ProductionQueueItem>(replanned);

					OptimizedStrategy = "Auto-Replanned (AI detected risk)";
				}
				else
				{
					OptimizedStrategy = "Stable Plan (No risks detected)";
				}

				OnPropertyChanged(nameof(Production.ProductionQueue));
				OnPropertyChanged(nameof(OptimizedStrategy));

				var capacity = _capacityEngine.CalculateCapacity(allWorkOrders);

				WorkCenters = new ObservableCollection<WorkCenter>(capacity);
				OnPropertyChanged(nameof(WorkCenters));
			}

			finally
			{
				_isRefreshing = false;
			}
		}

		public void RunScenarioSimulation(List<ProductionQueueItem> customQueue)
		{
			var scenarioService = new ProductionScenarioService();

			var result = scenarioService.SimulateScenario(customQueue);

			ScenarioPredictions = new ObservableCollection<ProductionCompletionPrediction>(result.Predictions);
			ScenarioLateJobs = result.LateJobs;
			ScenarioTotalDelay = result.TotalDelayDays;

			OnPropertyChanged(nameof(ScenarioPredictions));
			OnPropertyChanged(nameof(ScenarioLateJobs));
			OnPropertyChanged(nameof(ScenarioTotalDelay));
		}

		

		[RelayCommand]
		private void StartWorkOrder(Guid id)
		{
			var workOrder = Production.ProductionQueue
				.FirstOrDefault(w => w.Id == id);

			if (workOrder == null)
				return;

			if (ProductionBlocks.Any(b => b.WorkOrderNumber == workOrder.WorkOrderNumber))
			{
				System.Diagnostics.Debug.WriteLine($"❌ BLOCKED: {workOrder.WorkOrderNumber}");
				return;
			}

			if (Execution.RunningWorkOrders.Any(w => w.Id == id))
			{
				System.Diagnostics.Debug.WriteLine($"⚠ Already running: {workOrder.WorkOrderNumber}");
				return;
			}

			if (workOrder.Status != "Ready" &&
				workOrder.Status != "Paused" &&
				workOrder.Status != "0")
			{
				System.Diagnostics.Debug.WriteLine($"⚠ Not ready: {workOrder.WorkOrderNumber}");
				return;
			}

			System.Diagnostics.Debug.WriteLine($"▶ MANUAL START: {workOrder.WorkOrderNumber}");

			Execution.StartCommand.Execute(id);

			// 🔥 FORCE HARD RELOAD
			Execution.Load();
			LoadProductionQueue();
			LoadProductionReadiness();

			RefreshProductionSystem();

			ProductionChanged?.Invoke();
		}

		[RelayCommand]
		private void PauseWorkOrder(Guid id)
		{
			Execution.PauseWorkOrderCommand.Execute(id);

			RefreshProductionSystem();
			ProductionChanged?.Invoke();
		}

		[RelayCommand]
		private void CompleteWorkOrder(Guid id)
		{
			Execution.CompleteWorkOrderCommand.Execute(id);

			RefreshProductionSystem();
			ProductionChanged?.Invoke();
		}

		[RelayCommand]
		private void StartRecommended()
		{
			if (TopPriorityWorkOrder == null)
				return;

			if (TopPriorityWorkOrder.Status == "Blocked")
			{
				System.Diagnostics.Debug.WriteLine("⚠ Cannot start blocked work order");
				return;
			}

			// 🚫 Disabled for stability
			System.Diagnostics.Debug.WriteLine("AI Start Disabled");

			RefreshProductionSystem();
		}

		private ProductionQueueItem? _selectedWorkOrder;

		public ProductionQueueItem? SelectedWorkOrder
		{
			get => _selectedWorkOrder;
			set
			{
				if (_selectedWorkOrder == value)
					return;

				_selectedWorkOrder = value;

				if (value == null)
					return;

				Debug.WriteLine($"🔥 SELECTED WO: {value.WorkOrderNumber}");
				Debug.WriteLine($"FINAL RISKS: {DeadlineRisks.Count}");
				Debug.WriteLine($"TOWER RISKS: {ProductionControlTower.DeadlineRisks}");

				var wo = _workOrderRepository
					.GetAll()
					.FirstOrDefault(w => w.WorkOrderNumber == value.WorkOrderNumber);

				if (wo == null)
				{
					Debug.WriteLine("❌ WORK ORDER NOT FOUND");
					return;
				}

				// 🔥 LOAD MATERIAL TRACE (ONLY ONCE)
				Execution.LoadMaterialTrace(wo);

				// 🔥 UPDATE UI
				OnPropertyChanged(nameof(SelectedWorkOrderMaterials));
			}
		}
		private WorkOrder? GetWorkOrderByNumber(string number)
{
	return _workOrderRepository
		.GetAll()
		.FirstOrDefault(w => w.WorkOrderNumber == number);
}
public void RefreshDashboard()
{

			var allWorkOrders = _workOrderRepository.GetAll();
			Func<Guid, IEnumerable<MaterialRequirement>> getMaterials = (woId) =>
	{
		return _materialRepo.GetByWorkOrderId(woId)
			.Select(m => new MaterialRequirement
			{
				ItemCode = m.ItemCode,
				RequiredQuantity = m.RequiredQuantity
			});
	};

	Func<string, double> getStock = (code) =>
	{
		var stock = _allStock.FirstOrDefault(s => s.ItemCode == code);
		return stock?.Quantity ?? 0;
	};

	var risks = _globalRiskService.GetAllRisks(
		allWorkOrders,
		getMaterials,
		getStock
	);

	ProductionRisks = new ObservableCollection<ProductionRisk>(risks);
	OnPropertyChanged(nameof(ProductionRisks));
}

	} }

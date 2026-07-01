using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class ProductionSnapshot
    {
        public List<ProductionCompletionPrediction> CompletionPredictions { get; set; } = new();

        public ProductionQueueItem? TopPriorityWorkOrder { get; set; }

        // ===========================
        // Production Queue
        // ===========================

        public List<ProductionQueueItem> Queue { get; set; }
            = new();
        public List<ProductionQueueItem>BlockEvaluatedQueue { get; set; }
            = new();

        // ===========================
        // Control Tower
        // ===========================

        public ProductionControlTowerModel ControlTower { get; set; }
            = new();

        public List<WorkCenterStatus> WorkCenters { get; set; }
            = new();

        // ===========================
        // Capacity
        // ===========================

        public ObservableCollection<ProductionCapacityForecast> CapacityForecast { get; set; }
            = new();

        // ===========================
        // Risks
        // ===========================

        public ObservableCollection<DeadlineRisk> DeadlineRisks { get; set; }
            = new();

        // ===========================
        // Recommendations
        // ===========================

        public ObservableCollection<ProductionRecommendationModel> Recommendations { get; set; }
            = new();

        // ===========================
        // Delay Prediction
        // ===========================

        public ObservableCollection<ProductionDelayPrediction> DelayPredictions { get; set; }
            = new();

        // ===========================
        // Bottlenecks
        // ===========================

        public ObservableCollection<ProductionBottleneckModel> Bottlenecks { get; set; }
            = new();

        // ===========================
        // Material Shortages
        // ===========================

        public ObservableCollection<WorkOrderMaterialShortage> MaterialShortages { get; set; }
            = new();

        // ===========================
        // Health
        // ===========================

        public int HealthScore { get; set; }

        public double CapacityLoad { get; set; }

        public int CompletedToday { get; set; }

        public int CompletedThisWeek { get; set; }

        public string FactoryStatus { get; set; } = "Healthy";

        public List<ProductionBlock> ProductionBlocks { get; set; } = new();

        public ProductionReadinessResult? Readiness { get; set; }

        // ===========================
        // Production Summary
        // ===========================

        public int RunningWorkOrders { get; set; }

        public int BlockedWorkOrders { get; set; }

        public int ReadyWorkOrders { get; set; }
        public List<ProductionGanttItem> Timeline { get; set; }
            = new();

        public int CompletedWorkOrders { get; set; }
    }
}
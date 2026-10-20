using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services.Planning;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Production Engine Service
    ----------------------------------------------------------
    Purpose:
    Coordinates production services and produces a
    ProductionSnapshot that becomes the single source of truth.
    ==========================================================
    */

    public class ProductionEngineService
    {
        private readonly EvaluationEngine _evaluationEngine;
        private readonly SchedulingEngine _schedulingEngine;
        private readonly OptimizationEngine _optimizationEngine;
        private readonly RiskEngine _riskEngine;
        private readonly WorkCenterStatusService _workCenterStatusService;
        private readonly CapacityEngine _capacityEngine;
        private readonly RecommendationEngine _recommendationEngine;
        private readonly SummaryEngine _summaryEngine;
        private readonly TimelineEngine _timelineEngine;
        private readonly ProductionAlertService _alertService;
        private readonly ProductionControlTowerService _controlTowerService;

        private readonly ProductionBlockService _productionBlockService;
        private readonly ProductionDelayPredictionService _delayPredictionService;
        private readonly ProductionBottleneckDetectionService _bottleneckService;
        private readonly WorkOrderShortageDetectionService _shortageService;
        private readonly ProductionReadinessService _readinessService;
        public ProductionEngineService()
        {
            _productionBlockService = new ProductionBlockService();
            _delayPredictionService = new ProductionDelayPredictionService();
            _bottleneckService = new ProductionBottleneckDetectionService();
            _shortageService = new WorkOrderShortageDetectionService();
            _readinessService = new ProductionReadinessService(
                new WorkOrderRepository(),
                _shortageService);
            _evaluationEngine = new EvaluationEngine();
            _schedulingEngine = new SchedulingEngine();
            _optimizationEngine = new OptimizationEngine();
            _riskEngine = new RiskEngine();
            _workCenterStatusService = new WorkCenterStatusService();
            _capacityEngine = new CapacityEngine();
            _summaryEngine = new SummaryEngine();
            _recommendationEngine = new RecommendationEngine();
            _timelineEngine = new TimelineEngine();
            _alertService = new ProductionAlertService();
            _controlTowerService = new ProductionControlTowerService();
        }

        public ProductionRefreshResult Refresh()
        {
            var watch =
                System.Diagnostics.Stopwatch.StartNew();

            var result =
                new ProductionRefreshResult();

            try
            {
                var snapshot =
                    new ProductionSnapshot();

                _schedulingEngine.Build(snapshot);

                _evaluationEngine.Evaluate(snapshot);

                _optimizationEngine.Optimize(
                    snapshot,
                    new List<ProductionBottleneckModel>());

                _riskEngine.Evaluate(snapshot);

                _recommendationEngine.Evaluate(snapshot);

                _summaryEngine.Evaluate(snapshot);

                _capacityEngine.Evaluate(snapshot);

                snapshot.WorkCenters =
                    _workCenterStatusService.Build();

                _timelineEngine.Evaluate(snapshot);

                snapshot.Alerts =
                    new ObservableCollection<ProductionAlert>(
                        _alertService.GetAlerts());


                // Existing production services -> single snapshot source of truth
                snapshot.ProductionBlocks =
                    _productionBlockService.GetBlockedWorkOrders();

                snapshot.DelayPredictions =
                    new System.Collections.ObjectModel.ObservableCollection<ProductionDelayPrediction>(
                        _delayPredictionService.PredictDelays());

                snapshot.Bottlenecks =
                    new System.Collections.ObjectModel.ObservableCollection<ProductionBottleneckModel>(
                        _bottleneckService.DetectBottlenecks());

                snapshot.MaterialShortages =
                    new System.Collections.ObjectModel.ObservableCollection<WorkOrderMaterialShortage>(
                        _shortageService.DetectShortages());

                snapshot.Readiness =
                    _readinessService.Calculate();

                snapshot.ControlTower =
                    _controlTowerService.GetControlTower();


                // Keep the snapshot KPI aligned with the existing
                // Control Tower capacity calculation.
                snapshot.CapacityLoad =
                    snapshot.ControlTower.CapacityLoad;
                result.Snapshot = snapshot;

                result.Success = true;

                result.Messages.Add(
                    "Production refresh completed.");
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Messages.Add(ex.Message);
            }

            watch.Stop();

            result.Duration =
                watch.Elapsed;

            return result;
        }       
    }
}

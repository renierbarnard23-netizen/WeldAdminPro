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

        public ProductionEngineService()
        {
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

                snapshot.ControlTower =
                    _controlTowerService.GetControlTower();

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
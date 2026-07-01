using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Services.Planning;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class OptimizationEngine
    {
        private readonly AutoPriorityService _autoPriorityService;
        private readonly ProductionPriorityScoringService _priorityScoringService;
        private readonly ProductionReplanningService _replanningService;
        private readonly ProductionSimulationService _simulationService;

        public OptimizationEngine()
        {
            _autoPriorityService = new AutoPriorityService();
            _priorityScoringService = new ProductionPriorityScoringService();
            _replanningService = new ProductionReplanningService();
            _simulationService = new ProductionSimulationService();
        }

        public void Optimize(
            ProductionSnapshot snapshot,
            List<ProductionBottleneckModel> bottlenecks)
        {
            var reordered =
                _autoPriorityService.ReorderQueue(
                    snapshot.BlockEvaluatedQueue,
                    bottlenecks);

            reordered =
                _priorityScoringService.Score(
                    reordered,
                    bottlenecks);

            int priority = 1;

            foreach (var item in reordered)
            {
                item.Priority = priority++;
            }

            reordered =
                _replanningService.Replan(
                    reordered);

            snapshot.CompletionPredictions =
                _simulationService.SimulateSchedule(
                    reordered);

            snapshot.Queue = reordered;

            snapshot.TopPriorityWorkOrder =
                reordered.FirstOrDefault();

            if (snapshot.TopPriorityWorkOrder != null)
            {
                snapshot.TopPriorityWorkOrder.IsTopPriority = true;
            }
        }
    }
}
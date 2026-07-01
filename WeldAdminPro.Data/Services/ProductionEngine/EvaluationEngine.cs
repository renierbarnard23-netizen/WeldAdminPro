using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class EvaluationEngine
    {
        private readonly WorkOrderRepository _workOrderRepository;
        private readonly WorkOrderMaterialRepository _materialRepository;
        private readonly StockRepository _stockRepository;
        private readonly BlockReasonEngine _blockReasonEngine;

        public EvaluationEngine()
        {
            _workOrderRepository = new WorkOrderRepository();
            _materialRepository = new WorkOrderMaterialRepository();
            _stockRepository = new StockRepository();
            _blockReasonEngine = new BlockReasonEngine();
        }

        public void Evaluate(
            ProductionSnapshot snapshot)
        {
            snapshot.BlockEvaluatedQueue =
                new List<ProductionQueueItem>(
                    snapshot.Queue);

            EvaluateBlocks(
                snapshot.BlockEvaluatedQueue);
        }

        private void EvaluateBlocks(
            List<ProductionQueueItem> queue)
        {
            var workOrders =
                _workOrderRepository.GetAll().ToList();

            foreach (var item in queue)
            {
                var workOrder =
                    workOrders.FirstOrDefault(
                        w => w.WorkOrderNumber ==
                             item.WorkOrderNumber);

                if (workOrder == null)
                    continue;

                var materials =
                    _materialRepository
                        .GetByWorkOrderId(workOrder.Id);

                workOrder.MaterialRequirements =
                    materials.Select(m =>
                        new MaterialRequirement
                        {
                            ItemCode = m.ItemCode,
                            RequiredQuantity =
                                m.RequiredQuantity
                        }).ToList();

                var result =
                    _blockReasonEngine.Evaluate(
                        workOrder);

                item.BlockReason =
                    result.Reason;

                item.BlockMessage =
                    result.Message;
            }
        }
    }
}
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Summary Engine
    ----------------------------------------------------------
    Calculates the live production KPIs for the
    Production Control Tower.
    ==========================================================
    */

    public class SummaryEngine
    {
        private readonly WorkOrderRepository _workOrderRepository;

        public SummaryEngine()
        {
            _workOrderRepository = new WorkOrderRepository();
        }

        public void Evaluate(
            ProductionSnapshot snapshot)
        {
            var workOrders =
                _workOrderRepository
                    .GetAll()
                    .ToList();

            snapshot.RunningWorkOrders =
                workOrders.Count(w =>
                    w.Status == WorkOrderStatus.InProduction);

            snapshot.CompletedWorkOrders =
                workOrders.Count(w =>
                    w.Status == WorkOrderStatus.Completed);

            snapshot.ReadyWorkOrders =
                workOrders.Count(w =>
                    w.Status == WorkOrderStatus.Ready);

            snapshot.BlockedWorkOrders =
                snapshot.ProductionBlocks.Count;
        }
    }
}
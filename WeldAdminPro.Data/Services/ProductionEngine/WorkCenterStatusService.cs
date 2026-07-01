using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Work Center Status Service
    ----------------------------------------------------------
    Builds the live status of each work centre for the
    Production Control Tower.
    ==========================================================
    */

    public class WorkCenterStatusService
    {
        private readonly WorkOrderRepository _workOrderRepository;

        public WorkCenterStatusService()
        {
            _workOrderRepository = new WorkOrderRepository();
        }

        public List<WorkCenterStatus> Build()
        {
            var workOrders =
                _workOrderRepository
                    .GetAll()
                    .ToList();

            var result = new List<WorkCenterStatus>();

            result.Add(BuildCenter(
                "Welding Bay",
                workOrders));

            result.Add(BuildCenter(
                "Fabrication",
                workOrders));

            result.Add(BuildCenter(
                "Assembly",
                workOrders));

            result.Add(BuildCenter(
                "Quality Control",
                workOrders));

            return result;
        }

        private WorkCenterStatus BuildCenter(
            string name,
            List<WorkOrder> workOrders)
        {
            var running =
                workOrders.FirstOrDefault(w =>
                    w.Status == WorkOrderStatus.InProduction);

            return new WorkCenterStatus
            {
                Name = name,

                IsRunning = running != null,

                CurrentWorkOrder =
                    running?.WorkOrderNumber ?? "",

                QueueLength =
                    workOrders.Count,

                UtilizationPercent =
                    running == null ? 0 : 85,

                HoursRemaining =
                    running == null ? 0 : 8,

                IsBlocked = false
            };
        }
    }
}
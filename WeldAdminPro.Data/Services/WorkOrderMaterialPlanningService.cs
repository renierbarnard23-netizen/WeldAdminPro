using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class WorkOrderMaterialPlanningService
    {
        private readonly WorkOrderRepository _workOrderRepo;
        private readonly WorkOrderMaterialRepository _materialRepo;
        private readonly StockRepository _stockRepo;

        public WorkOrderMaterialPlanningService()
        {
            _workOrderRepo = new WorkOrderRepository();
            _materialRepo = new WorkOrderMaterialRepository();
            _stockRepo = new StockRepository();
        }

        public List<WorkOrderMaterialPlan> BuildPlan()
        {
            var result =
                new List<WorkOrderMaterialPlan>();

            var workOrders =
                _workOrderRepo.GetAll();

            var stock =
                _stockRepo.GetAll().ToList();

            foreach (var wo in workOrders)
            {
                var materials =
                    _materialRepo.GetByWorkOrderId(
                        wo.Id);

                foreach (var m in materials)
                {
                    var item =
                        stock.FirstOrDefault(
                            s => s.ItemCode ==
                                 m.ItemCode);

                    int available =
                        (int)Math.Floor(
                            item?.Quantity ?? 0);

                    result.Add(
                        new WorkOrderMaterialPlan
                        {
                            WorkOrderNumber =
                                wo.WorkOrderNumber,

                            ItemCode =
                                m.ItemCode,

                            Description =
                                item?.Description ?? "",

                            RequiredQuantity =
                            (int)Math.Floor(
                                m.RequiredQuantity),

                            StockAvailable =
                                available
                        });
                }
            }

            return result;
        }
        
    }
}
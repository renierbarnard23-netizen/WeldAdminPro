using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class MaterialReservationService
    {
        private readonly ReservedMaterialRepository _repo;
        private readonly WorkOrderRepository _workOrderRepo;
        private readonly StockRepository _stockRepo;

        public MaterialReservationService()
        {
            _repo =
                new ReservedMaterialRepository();

            _workOrderRepo =
                new WorkOrderRepository();

            _stockRepo =
                new StockRepository();
        }

        public List<MaterialReservation> GenerateReservations()
        {
            var reservations =
                _repo.GetAll();

            var workOrders =
                _workOrderRepo.GetAll();

            var stock =
                _stockRepo.GetAll();

            return reservations
                .Select(r =>
                {
                    var wo =
                        workOrders.FirstOrDefault(
                            w => w.Id == r.WorkOrderId);

                    var item =
                        stock.FirstOrDefault(
                            s => s.ItemCode == r.ItemCode);

                    return new MaterialReservation
                    {
                        WorkOrderNumber =
                            wo?.WorkOrderNumber ?? "",

                        ItemCode =
                            r.ItemCode,

                        RequiredQuantity =
                            (double)r.Quantity,

                        ReservedQuantity =
                            (double)r.Quantity,

                        AvailableStock =
                            (item?.Quantity ?? 0)
                            - (double)r.Quantity,

                        ReservationSuccessful =
                            true,

                        Reason =
                            "Reserved"
                    };
                })
                .ToList();
        }
    }
}
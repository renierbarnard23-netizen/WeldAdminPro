using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class PersistentReservationService
    {
        private readonly
            ReservedMaterialRepository _repo;

        private readonly
            WorkOrderMaterialRepository _materialRepo;

        public PersistentReservationService()
        {
            _repo =
                new ReservedMaterialRepository();

            _materialRepo =
                new WorkOrderMaterialRepository();
        }

        public void Reserve(
            Guid workOrderId)
        {
            _repo.DeleteByWorkOrder(
                workOrderId);

            var materials =
                _materialRepo
                    .GetByWorkOrderId(
                        workOrderId);

            foreach (var mat in materials)
            {
                _repo.Add(
                    new ReservedMaterial
                    {
                        Id =
                            Guid.NewGuid(),

                        WorkOrderId =
                            workOrderId,

                        ItemCode =
                            mat.ItemCode,

                        Quantity =
                            (decimal)mat.RequiredQuantity,

                        ReservedOn =
                            DateTime.UtcNow
                    });
            }
        }

        public void Release(
            Guid workOrderId)
        {
            _repo.DeleteByWorkOrder(
                workOrderId);
        }
    }
}
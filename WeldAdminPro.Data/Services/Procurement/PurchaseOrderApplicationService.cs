using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Procurement
{
    public class PurchaseOrderApplicationService
    {
        private readonly PurchaseOrderRepository _repository;

        private readonly SmartPurchaseOrderService _smartService;

        public PurchaseOrderApplicationService()
        {
            _repository = new PurchaseOrderRepository();

            _smartService = new SmartPurchaseOrderService();
        }

        public List<PurchaseOrder> GetAll()
        {
            return _repository.GetAll();
        }

        public List<PurchaseOrder> GetByProject(Guid projectId)
        {
            return _repository.GetByProject(projectId);
        }

        public void Save(PurchaseOrder purchaseOrder)
        {
            _repository.Save(purchaseOrder);
        }

        public string GenerateNextPONumber(int jobNumber)
        {
            return _repository.GenerateNextPONumber(jobNumber);
        }
        public PurchaseOrder? GetById(Guid id)
        {
            return _repository.GetById(id);
        }
        public PurchaseOrder? GenerateSmartPurchaseOrder(Project project, string supplier)
        {
            return _smartService.GenerateAutoPO(project, supplier);
        }
    }
}
using System;
using System.Linq;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class CostTrackingService
    {
        private readonly StockRepository _stockRepo;

        public CostTrackingService()
        {
            _stockRepo = new StockRepository();
        }

        /// <summary>
        /// Gets total material cost for a specific Work Order
        /// </summary>
        public decimal GetWorkOrderMaterialCost(string workOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(workOrderNumber))
                return 0;

            var transactions = _stockRepo.GetAllTransactions()
                .Where(t => t.Reference == workOrderNumber && t.Type == "OUT");

            return transactions.Sum(t => t.TransactionValue);
        }

        /// <summary>
        /// Gets total material cost for a Project
        /// </summary>
        public decimal GetProjectMaterialCost(Guid projectId)
        {
            if (projectId == Guid.Empty)
                return 0;

            var transactions = _stockRepo.GetAllTransactions()
                .Where(t => t.ProjectId == projectId && t.Type == "OUT");

            return transactions.Sum(t => t.TransactionValue);
        }
    }
}
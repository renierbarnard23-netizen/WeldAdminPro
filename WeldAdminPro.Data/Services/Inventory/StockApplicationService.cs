using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Reporting;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Inventory
{
    public class StockApplicationService
    {
        private readonly StockRepository _stockRepository;
        private readonly CategoryRepository _categoryRepository;

        private readonly LedgerIntegrityService _ledgerIntegrityService;
        private readonly LedgerRepairService _ledgerRepairService;

        private readonly StockAnalyticsService _stockAnalyticsService;
        private readonly ExecutiveReportService _executiveReportService;

        private readonly AuditLogRepository _auditRepository;

        private readonly StockForecastService _stockForecastService;

        private readonly MaterialTrendService _materialTrendService;

        private readonly ProjectRiskService _projectRiskService;

        private readonly SmartReorderPlannerService _smartReorderPlannerService;

        private readonly MaterialCostAnalysisService _materialCostService;
        public StockApplicationService()
        {
            _stockRepository = new StockRepository();
            _categoryRepository = new CategoryRepository();

            _ledgerIntegrityService = new LedgerIntegrityService();
            _ledgerRepairService = new LedgerRepairService();

            _stockAnalyticsService = new StockAnalyticsService();

            _executiveReportService = new ExecutiveReportService();

            _auditRepository =
                new AuditLogRepository(DatabasePath.GetConnectionString());

            _stockForecastService = new StockForecastService();

            _materialTrendService = new MaterialTrendService();

            _projectRiskService = new ProjectRiskService();

            _smartReorderPlannerService = new SmartReorderPlannerService();

            _materialCostService = new MaterialCostAnalysisService();
        }

        // ==========================
        // STOCK
        // ==========================

        public List<StockItem> GetStockItems()
        {
            return _stockRepository.GetAll();
        }

        public StockItem? GetStockItem(Guid id)
        {
            return _stockRepository.GetById(id);
        }
        public StockItem? GetByItemCode(string itemCode)
        {
            return _stockRepository.GetByItemCode(itemCode);
        }

        public void SaveStockItem(StockItem item)
        {
            var existing = _stockRepository.GetById(item.Id);

            if (existing == null)
            {
                _stockRepository.Add(item);
            }
            else
            {
                _stockRepository.Update(item);
            }
        }


        public string GetNextItemCode()
        {
            return _stockRepository.GetNextItemCodeSuggestion();
        }

        public int GetAvailableQuantity(Guid id)
        {
            return _stockRepository.GetAvailableQuantity(id);
        }

        // ==========================
        // CATEGORIES
        // ==========================

        public List<Category> GetCategories()
        {
            return _categoryRepository
                .GetAllActive()
                .ToList();
        }

        // ==========================
        // TRANSACTIONS
        // ==========================

        public List<StockTransaction> GetTransactions()
        {
            return _stockRepository.GetAllTransactions();
        }

        public List<StockTransaction> GetProjectTransactions(Guid projectId)
        {
            return _stockRepository.GetProjectTransactions(projectId);
        }

        // ==========================
        // LEDGER
        // ==========================

        public void RecalculateLedger()
        {
            _stockRepository.RecalculateAllBalances();
        }


        // ==========================
        // LEDGER HEALTH
        // ==========================

        public (bool IsValid, int ErrorCount) ValidateLedger()
        {
            return _ledgerIntegrityService.ValidateLedger();
        }

        public int RepairLedger()
        {
            return _ledgerRepairService.RepairLedger();
        }

        // ==========================
        // ANALYTICS
        // ==========================

        // To be completed after reviewing
        // StockAnalyticsService

        // ==========================
        // EXECUTIVE REPORTING
        // ==========================

        // To be completed after reviewing
        // ExecutiveReportService

        // ==========================
        // AUDIT
        // ==========================

        public List<AuditLog> GetAuditLogs()
        {
            return _auditRepository.GetAll();
        }

        // ==========================
        // STOCK ITEM CREATION
        // ==========================

        public StockItem CreateNewStockItem()
        {
            return new StockItem
            {
                Id = Guid.NewGuid(),
                ItemCode = _stockRepository.GetNextItemCodeSuggestion(),
                Quantity = 0,
                Category = "Uncategorised"
            };
        }

        // ==========================
        // INVENTORY SERVICES
        // ==========================

        public void ReceiveStock(StockTransaction transaction)
        {
            _stockRepository.AddTransaction(transaction);

            AuditService.Log(
                "Receive Stock",
                "Inventory",
                transaction.StockItemId.ToString());
        }

        public void IssueStock(StockTransaction transaction)
        {
            _stockRepository.AddTransaction(transaction);

            AuditService.Log(
                "Issue Stock",
                "Inventory",
                transaction.StockItemId.ToString());
        }
        public List<StockTransaction> GetTransactions(
            DateTime? start,
            DateTime? end)
        {
            return _stockRepository
                .GetTransactionsByDateRange(start, end);
        }
        public List<StockTransaction> GetAuditTransactionLog()
        {
            return _stockRepository.GetAuditLog();
        }

        public List<WorkOrderMaterialTrace> GetMaterialTrace(
            string workOrderNumber)
        {
            return _stockRepository
                .GetMaterialTraceForWorkOrder(workOrderNumber);
        }

        public IEnumerable<ReturnableItemDto> GetReturnableItems(
            Guid projectId)
        {
            return _stockRepository
                .GetReturnableItems(projectId);
        }

        public List<StockTransaction> GetIssuedMaterials(
            Guid projectId)
        {
            return _stockRepository
                .GetIssuedMaterials(projectId);
        }
        public List<StockForecastModel> GetStockForecast()
        {
            return _stockForecastService.GetStockForecast();
        }

        public List<MaterialTrendModel> GetMaterialTrends()
        {
            return _materialTrendService.GetMaterialTrends();
        }

        public List<ProjectRiskModel> GetProjectRiskSummary()
        {
            return _projectRiskService.GetProjectRiskSummary();
        }
        public List<StockItem> GetSmartReorderItems()
        {
            var items = _stockRepository.GetAll();

            return _smartReorderPlannerService
                .GetReorderItems(items);
        }
        public List<MaterialCostDriver> GetMaterialCostDrivers()
        {
            return _materialCostService.GetTopMaterialCostDrivers();
        }

    }
}
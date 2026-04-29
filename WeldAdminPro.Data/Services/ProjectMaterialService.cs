using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProjectMaterialService
	{
		private readonly StockRepository _stockRepository;
		private readonly StockProjectTransactionService _transactionService;

		public ProjectMaterialService()
		{
			_stockRepository = new StockRepository();
			_transactionService = new StockProjectTransactionService();
		}

		// ================= STOCK ITEMS =================

		public IEnumerable<StockItem> GetStockItems()
		{
			return _stockRepository.GetAll();
		}

		// ================= ISSUE MATERIAL =================

		public void IssueMaterial(Project project, StockItem item, decimal qty, string issuedBy)
		{
			_transactionService.IssueStock(
				project,
				item,
				qty,
				issuedBy);
		}

		// ================= RETURN MATERIAL =================

		public void ReturnMaterial(Project project, StockItem item, decimal qty, decimal cost, string issuedBy)
		{
			_transactionService.ReturnStock(
				project,
				item,
				qty,
				cost,
				issuedBy);
		}

		// ================= PROJECT TRANSACTIONS =================

		public IEnumerable<StockTransaction> GetProjectTransactions(Guid projectId)
		{
			return _stockRepository.GetProjectTransactions(projectId);
		}

        // ================= RETURNABLE ITEMS =================

        public IEnumerable<ReturnableItemDto> GetReturnableItems(Guid projectId)
        {
			return _stockRepository.GetReturnableItems(projectId);
		}

		// ================= ISSUED ITEMS =================

		public IEnumerable<StockTransaction> GetIssuedItems(Guid projectId)
		{
			return _stockRepository.GetIssuedMaterials(projectId);
		}
	}
}
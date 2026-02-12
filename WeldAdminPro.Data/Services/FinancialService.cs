using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class FinancialService
	{
		private readonly StockRepository _stockRepository;

		public FinancialService()
		{
			_stockRepository = new StockRepository();
		}

		// =========================
		// ISSUE COST CALCULATION
		// =========================
		public decimal CalculateIssueCost(Guid stockItemId, decimal quantity)
		{
			var stockItems = _stockRepository.GetAll();
			var stock = stockItems.Find(s => s.Id == stockItemId);

			if (stock == null)
				throw new Exception("Stock item not found for costing.");

			return quantity * stock.AverageUnitCost;
		}

		// =========================
		// APPLY ISSUE TO PROJECT
		// =========================
		public void ApplyIssueCost(Project project, Guid stockItemId, decimal quantity)
		{
			var cost = CalculateIssueCost(stockItemId, quantity);

			project.ActualCost += cost;
			project.LastModifiedOn = DateTime.UtcNow;
		}

		// =========================
		// APPLY RETURN TO PROJECT
		// ✅ EXACT COST REVERSAL
		// =========================
		public void ApplyReturnCost(
			Project project,
			Guid stockItemId,
			decimal quantity,
			decimal unitCostAtIssue)
		{
			// Reverse the EXACT cost that was originally applied
			var cost = quantity * unitCostAtIssue;

			project.ActualCost -= cost;

			if (project.ActualCost < 0)
				project.ActualCost = 0;

			project.LastModifiedOn = DateTime.UtcNow;
		}

		// =========================
		// VARIANCE
		// =========================
		public decimal CalculateVariance(Project project)
		{
			return project.Budget - project.ActualCost;
		}

		// =========================
		// MARGIN %
		// =========================
		public decimal CalculateMarginPercentage(Project project)
		{
			if (project.Budget == 0)
				return 0;

			var variance = CalculateVariance(project);
			return (variance / project.Budget) * 100m;
		}
	}
}

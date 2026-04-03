using System;
using Microsoft.Data.Sqlite;
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
		// INTERNAL ROUNDING SAFETY
		// =========================
		private decimal RoundCurrency(decimal value)
		{
			return Math.Round(value, 2, MidpointRounding.AwayFromZero);
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

			var cost = quantity * stock.AverageUnitCost;
			return RoundCurrency(cost);
		}

		// =========================
		// APPLY ISSUE TO PROJECT
		// =========================
		public void ApplyIssueCost(Project project, Guid stockItemId, decimal quantity)
		{
			var cost = CalculateIssueCost(stockItemId, quantity);

			project.ActualCost = RoundCurrency(project.ActualCost + cost);

			if (project.ActualCost < 0)
				project.ActualCost = 0;

			project.LastModifiedOn = DateTime.UtcNow;
		}

		// =========================
		// APPLY RETURN (EXACT COST)
		// =========================
		public void ApplyReturnCost(Project project,
									Guid stockItemId,
									decimal quantity,
									decimal unitCostAtIssue)
		{
			var cost = RoundCurrency(quantity * unitCostAtIssue);

			project.ActualCost = RoundCurrency(project.ActualCost - cost);

			if (project.ActualCost < 0)
				project.ActualCost = 0;

			project.LastModifiedOn = DateTime.UtcNow;
		}

		// =========================
		// VARIANCE
		// =========================
		public decimal CalculateVariance(Project project)
		{
			return RoundCurrency(project.Budget - project.ActualCost);
		}

		// =========================
		// MARGIN %
		// =========================
		public decimal CalculateMarginPercentage(Project project)
		{
			if (project.Budget == 0)
				return 0;

			var variance = CalculateVariance(project);
			return RoundCurrency((variance / project.Budget) * 100m);
		}
		public decimal CalculateProjectActualCost(Guid projectId)
		{
			using var connection = new SqliteConnection(
				$"Data Source={DatabasePath.Get()}");

			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
SELECT
    IFNULL(SUM(
        CASE
            WHEN Type = 'OUT' THEN Quantity * UnitCost
            WHEN Type = 'IN'  THEN -Quantity * UnitCost
        END
    ), 0)
FROM StockTransactions
WHERE LOWER(ProjectId) = LOWER($projectId);";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			var result = cmd.ExecuteScalar();

			decimal value = result == null || result == DBNull.Value
				? 0m
				: Convert.ToDecimal(result);

			return Math.Round(value, 2, MidpointRounding.AwayFromZero);
		}

	}

}

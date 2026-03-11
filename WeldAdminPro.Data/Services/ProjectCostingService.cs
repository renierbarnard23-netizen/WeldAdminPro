using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProjectCostingService
	{
		private readonly StockRepository _repository;

		public ProjectCostingService()
		{
			_repository = new StockRepository();
		}

		public List<ProjectCostSummary> GetProjectCostSummary()
		{
			var transactions = _repository.GetAllTransactions();

			return transactions
				.Where(t => t.Type == "OUT" && t.ProjectId != null)
				.GroupBy(t => new { t.ProjectId, t.ProjectName })
				.Select(g => new ProjectCostSummary
				{
					ProjectId = g.Key.ProjectId ?? Guid.Empty,
					ProjectName = g.Key.ProjectName ?? "Unknown",
					TotalUnitsConsumed = g.Sum(x => x.Quantity),
					TotalMaterialCost = g.Sum(x => x.TransactionValue)
				})
				.OrderByDescending(x => x.TotalMaterialCost)
				.ToList();
		}
		public List<ProjectMaterialBreakdown> GetProjectMaterialBreakdown(Guid projectId)
		{
			var transactions = _repository.GetAllTransactions();

			return transactions
				.Where(t => t.Type == "OUT" && t.ProjectId == projectId)
				.GroupBy(t => new { t.ItemCode, t.ItemDescription })
				.Select(g => new ProjectMaterialBreakdown
				{
					ItemCode = g.Key.ItemCode,
					Description = g.Key.ItemDescription,
					QuantityUsed = g.Sum(x => x.Quantity),
					MaterialCost = g.Sum(x => x.TransactionValue)
				})
				.OrderByDescending(x => x.MaterialCost)
				.ToList();
		}
	}
}
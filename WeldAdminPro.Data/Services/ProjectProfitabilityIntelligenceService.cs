using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProjectProfitabilityIntelligenceService
	{
		private readonly StockTransactionRepository _transactionRepository;
		private readonly ProjectRepository _projectRepository;

		public ProjectProfitabilityIntelligenceService()
		{
			_transactionRepository = new StockTransactionRepository();
			_projectRepository = new ProjectRepository();
		}

		public List<ProjectProfitabilityStat> GetProjectProfitability()
		{
			var transactions = _transactionRepository.GetAllTransactions();
			var projects = _projectRepository.GetAll();

			var stats = projects.Select(p =>
			{
				var materialCost = transactions
					.Where(t => t.ProjectId == p.Id && t.Type == "OUT")
					.Sum(t => t.Quantity * t.UnitCost);

				return new ProjectProfitabilityStat
				{
					ProjectName = p.ProjectName ?? "",
					Revenue = p.Budget,
					MaterialCost = materialCost
				};
			})
			.OrderByDescending(p => p.Profit) // uses computed property
			.ToList();

			return stats;
		}
	}
}
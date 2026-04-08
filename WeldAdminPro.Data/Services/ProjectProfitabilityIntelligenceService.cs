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
        private readonly ProjectStockUsageRepository _usageRepository;

        public ProjectProfitabilityIntelligenceService()
		{
			_transactionRepository = new StockTransactionRepository();
			_projectRepository = new ProjectRepository();
            _usageRepository = new ProjectStockUsageRepository(); // ✅ ADD THIS
        }

        public List<ProjectProfitabilityStat> GetProjectProfitability()
        {
            var projects = _projectRepository.GetAll();

            var stats = projects.Select(p =>
            {
                var materialCost = _usageRepository
                    .GetByProjectId(p.Id)
                    .Sum(u => u.LineCost);

                return new ProjectProfitabilityStat
                {
                    ProjectName = p.ProjectName ?? "",
                    Revenue = p.Budget,
                    MaterialCost = materialCost
                };
            })
            .OrderByDescending(p => p.Profit)
            .ToList();

            return stats;
        }

    }
}
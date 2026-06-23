using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
	public class ProjectRiskService
	{
		private readonly ProjectRepository _projectRepository;
		private readonly ProjectCostingService _projectCostingService;

		public ProjectRiskService()
		{
			_projectRepository = new ProjectRepository();
			_projectCostingService = new ProjectCostingService();
		}

		public List<ProjectRiskModel> GetProjectRiskSummary()
		{
			var projects = _projectRepository.GetAll().ToList();
			var costing = _projectCostingService.GetProjectCostSummary();

			var results = new List<ProjectRiskModel>();

			foreach (var project in projects)
			{
				var cost = costing
					.FirstOrDefault(c => c.ProjectId == project.Id);

				decimal materialCost = cost?.TotalMaterialCost ?? 0;

				decimal budgetUsedPercent = 0;

				if (project.Budget > 0)
					budgetUsedPercent = (materialCost / project.Budget) * 100;

				results.Add(new ProjectRiskModel
				{
					ProjectId = project.Id.ToString(),
					ProjectName = project.ProjectName,
					Budget = project.Budget,
					MaterialCost = materialCost,
					BudgetUsedPercent = budgetUsedPercent,
					RiskLevel = CalculateRiskLevel(budgetUsedPercent)
				});
			}

			return results
				.OrderByDescending(r => r.BudgetUsedPercent)
				.ToList();
		}

        public ComplianceRiskModel EvaluateCompliance(ProjectComplianceResult result)
        {
            int score = 100;

            // DOCUMENT IMPACT
            score -= (int)((100 - result.DocumentCompliancePercent) * 0.6);

            // WPS IMPACT
            score -= (int)((100 - result.WpsCompliancePercentage) * 0.4);

            // ISSUE PENALTY
            score -= result.Issues.Count * 2;

            if (score < 0) score = 0;

            return new ComplianceRiskModel
            {
                Score = score,
                Level = score >= 85 ? "LOW"
                      : score >= 60 ? "MEDIUM"
                      : "HIGH",

                Color = score >= 85 ? "Green"
                      : score >= 60 ? "Orange"
                      : "Red"
            };
        }

        private string CalculateRiskLevel(decimal percent)
		{
			if (percent >= 95)
				return "Critical";

			if (percent >= 80)
				return "High";

			if (percent >= 50)
				return "Medium";

			return "Low";
		}
	}
}
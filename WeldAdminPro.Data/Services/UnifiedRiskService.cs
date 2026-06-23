using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class UnifiedRiskService
    {
        private readonly ProjectCostingService _costingService = new();
        private readonly ProjectRiskService _riskService = new(); // for compliance
        private readonly ProjectRepository _projectRepo = new();

        public UnifiedProjectRiskModel Evaluate(ProjectComplianceResult compliance, System.Guid projectId)
        {
            var complianceRisk = _riskService.EvaluateCompliance(compliance);
            int complianceScore = complianceRisk.Score;

            var costing = _costingService.GetProjectCostSummary()
                .FirstOrDefault(c => c.ProjectId == projectId);

            var project = _projectRepo.GetById(projectId);

            int financialScore = 100;

            if (project != null && project.Budget > 0 && costing != null)
            {
                var percent = (double)(costing.TotalMaterialCost / project.Budget) * 100;

                financialScore = percent switch
                {
                    >= 100 => 20,
                    >= 90 => 40,
                    >= 75 => 60,
                    >= 50 => 80,
                    _ => 100
                };
            }

            int finalScore =
                (int)(complianceScore * 0.6 +
                      financialScore * 0.4);

            string level =
                finalScore >= 85 ? "LOW"
              : finalScore >= 60 ? "MEDIUM"
              : "HIGH";

            string color =
                finalScore >= 85 ? "Green"
              : finalScore >= 60 ? "Orange"
              : "Red";

            return new UnifiedProjectRiskModel
            {
                Score = finalScore,
                Level = level,
                Color = color,

                ComplianceScore = complianceScore,
                FinancialScore = financialScore
            };
        }
        
    }
}
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class UnifiedRiskService
    {
        private readonly ProjectCostingService _costingService = new();
        private readonly ProjectRiskService _riskService = new(); // for compliance

        public UnifiedProjectRiskModel Evaluate(ProjectComplianceResult compliance, Project project)
        {
            var complianceRisk = _riskService.EvaluateCompliance(compliance);
            int complianceScore = complianceRisk.Score;

            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (project.Id == Guid.Empty)
                throw new ArgumentException(
                    "Project Id is required.",
                    nameof(project));

            var materialBreakdown =
                _costingService.GetProjectMaterialBreakdown(project.Id);

            var materialCost =
                materialBreakdown.Sum(x => x.MaterialCost);

            int financialScore = 100;

            if (project.Budget > 0 && materialCost > 0)
            {
                var percent = (double)(materialCost / project.Budget) * 100;

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

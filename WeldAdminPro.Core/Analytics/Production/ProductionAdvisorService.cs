using System.Collections.Generic;

namespace WeldAdminPro.Core.Analytics.Production
{
    public class ProductionAdvisorService
    {
        public List<ProductionRecommendation>
            GetRecommendations(
                ProductionControlTowerModel model)
        {
            var recommendations =
                new List<ProductionRecommendation>();

            if (model.MaterialShortages > 0)
            {
                recommendations.Add(
                    new ProductionRecommendation
                    {
                        Severity = "Critical",
                        Message =
                            "Purchase materials immediately."
                    });
            }

            if (model.CapacityLoad >= 100)
            {
                recommendations.Add(
                    new ProductionRecommendation
                    {
                        Severity = "Warning",
                        Message =
                            "Capacity exceeded. Add shifts or reschedule."
                    });
            }

            if (model.DeadlineRisks > 0)
            {
                recommendations.Add(
                    new ProductionRecommendation
                    {
                        Severity = "Warning",
                        Message =
                            "Review at-risk work orders."
                    });
            }

            if (!recommendations.Any())
            {
                recommendations.Add(
                    new ProductionRecommendation
                    {
                        Severity = "Healthy",
                        Message =
                            "Production system operating normally."
                    });
            }

            return recommendations;
        }
    }
}
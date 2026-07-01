using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class RecommendationEngine
    {
        public void Evaluate(
    ProductionSnapshot snapshot)
        {
            var recommendations =
                new ObservableCollection<ProductionRecommendationModel>();

            // -------------------------------------------------
            // Highest Priority Work Order
            // -------------------------------------------------

            if (snapshot.TopPriorityWorkOrder != null)
            {
                recommendations.Add(
                    new ProductionRecommendationModel
                    {
                        WorkOrderNumber =
                            snapshot.TopPriorityWorkOrder.WorkOrderNumber,

                        Recommendation =
                            $"Start {snapshot.TopPriorityWorkOrder.WorkOrderNumber}",

                        Explanation =
                            "Highest ranked work order after optimization.",

                        Score = 100
                    });
            }

            // -------------------------------------------------
            // Deadline Risks
            // -------------------------------------------------

            foreach (var risk in snapshot.DeadlineRisks)
            {
                recommendations.Add(
                    new ProductionRecommendationModel
                    {
                        WorkOrderNumber = risk.WorkOrderNumber,

                        Recommendation =
                            "Prioritize immediately",

                        Explanation =
                            "Work order is approaching its delivery deadline.",

                        Score = 90
                    });
            }

            // -------------------------------------------------
            // Material Shortages
            // -------------------------------------------------

            foreach (var shortage in snapshot.MaterialShortages)
            {
                recommendations.Add(
                    new ProductionRecommendationModel
                    {
                        WorkOrderNumber =
                            shortage.WorkOrderNumber,

                        Recommendation =
                            "Procure missing material",

                        Explanation =
                            "Production cannot continue until required material is available.",

                        Score = 80
                    });
            }

            // -------------------------------------------------
            // Production Blocks
            // -------------------------------------------------

            foreach (var block in snapshot.ProductionBlocks)
            {
                recommendations.Add(
                    new ProductionRecommendationModel
                    {
                        WorkOrderNumber =
                            block.WorkOrderNumber,

                        Recommendation =
                            "Resolve production block",

                        Explanation =
                            block.BlockingItemsText,

                        Score = 95
                    });
            }

            snapshot.Recommendations = recommendations;
        }
    }
}
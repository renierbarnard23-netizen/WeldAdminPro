using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class TurnoverReadinessEngine
    {
        public TurnoverReadinessResult Evaluate(
        List<Weld> welds,
        List<RepairRecord> repairs,
        int missingDocuments)
        {
            var result =
            new TurnoverReadinessResult();

            result.OpenWelds =
            welds.Count(x =>
                x.WorkflowStatus !=
                WeldWorkflowStatus.Closed);

            result.OpenRepairs =
                repairs.Count(x =>
                    x.Status !=
                    RepairStatus.Closed);

            result.PendingReinspections =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.ReinspectionRequired);

            result.MissingDocuments =
                missingDocuments;

            if (result.OpenWelds > 0)
            {
                result.BlockingIssues.Add(
                    $"{result.OpenWelds} welds are not closed.");
            }

            if (result.OpenRepairs > 0)
            {
                result.BlockingIssues.Add(
                    $"{result.OpenRepairs} repairs are still open.");
            }

            if (result.PendingReinspections > 0)
            {
                result.BlockingIssues.Add(
                    $"{result.PendingReinspections} welds require reinspection.");
            }

            if (result.MissingDocuments > 0)
            {
                result.BlockingIssues.Add(
                    $"{result.MissingDocuments} required documents are missing.");
            }

            result.IsReady =
                !result.BlockingIssues.Any();

            return result;
        }
    }
}

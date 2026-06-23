using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class QaDashboardAnalyticsService
    {
        public QaDashboardMetrics Generate(
        List<Weld> welds,
        List<RepairRecord> repairs)
        {
            var metrics =
            new QaDashboardMetrics();

            metrics.TotalWelds =
                welds.Count;

            metrics.AcceptedWelds =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.Accepted);

            metrics.WeldsUnderRepair =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.RepairRequired
                    ||
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.UnderRepair);

            metrics.PendingReinspection =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.ReinspectionRequired);

            metrics.ClosedWelds =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.Closed);

            metrics.OpenRepairs =
                repairs.Count(x =>
                    x.Status != RepairStatus.Closed);

            metrics.CompletedRepairs =
                repairs.Count(x =>
                    x.Status == RepairStatus.Accepted);

            metrics.RejectedRepairs =
                repairs.Count(x =>
                    x.Status == RepairStatus.Rejected);

            if (metrics.TotalWelds > 0)
            {
                metrics.RepairRate =
                    (double)metrics.OpenRepairs
                    / metrics.TotalWelds
                    * 100;
            }

            return metrics;
        }

        public List<WorkflowDistributionItem>
            GetWorkflowDistribution(
            List<Weld> welds)
        {
            return welds
            .GroupBy(x => x.WorkflowStatus)
            .Select(x =>
            new WorkflowDistributionItem
            {
                Status =
            x.Key.ToString(),

            Count =
                            x.Count()
            })
                .OrderByDescending(x => x.Count)
                .ToList();

        }

        public List<RepairAgingItem>
            GetRepairAging(
            List<RepairRecord> repairs)
        {
            return repairs
            .Select(x =>
            new RepairAgingItem
            {
                RepairId =
            x.Id,

            WeldId =
                            x.WeldId,

                RepairNumber =
                            x.RepairNumber,

                Status =
                            x.Status.ToString(),

                AgeDays =
                            (DateTime.UtcNow
                            - x.RequestedDate).Days,

                IsOverdue =
                            (DateTime.UtcNow
                            - x.RequestedDate).Days > 7
            })
                .OrderByDescending(x => x.AgeDays)
                .ToList();
}


    }
}

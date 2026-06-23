using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldControlTowerService
    {  
        public WeldControlTowerMetrics Generate(
            List<Weld> welds)
        {

            var metrics =
                new WeldControlTowerMetrics();

            if (welds == null || !welds.Any())
                return metrics;

            metrics.TotalWelds =
                welds.Count;

            metrics.DraftCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.Draft);

            metrics.NdtPendingCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.NdtPending);

            metrics.RepairRequiredCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.RepairRequired);

            metrics.ReleasedCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.Released);

            metrics.ClosedCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                    WeldWorkflowStatus.Closed);

            metrics.BlockedCount =
                welds.Count(x =>
                    !x.ReleaseReady
                    &&
                    x.WorkflowStatus != WeldWorkflowStatus.Closed
                    &&
                    x.WorkflowStatus != WeldWorkflowStatus.Released);

            metrics.BlockedWelds =
                welds
                    .Where(x =>
                        !x.ReleaseReady &&
                        x.WorkflowStatus != WeldWorkflowStatus.Closed &&
                        x.WorkflowStatus != WeldWorkflowStatus.Released)
                    .Select(x =>
                        new BlockedWeldItem
                        {
                            WeldId = x.Id,

                            WeldNumber =
                                x.WeldNumber,

                            WorkflowStatus =
                                x.WorkflowStatus
                            .ToString(),

                            BlockingReason =
                                string.IsNullOrWhiteSpace(
                                    x.ReadinessSummary)
                                ? "Blocked"
                                : x.ReadinessSummary
                        })
                    .ToList();

            metrics.OverdueNdtCount =
                welds.Count(x =>
                    x.WorkflowStatus ==
                        WeldWorkflowStatus.NdtPending
                    &&
                    x.LastNdtDate.HasValue
                    &&
                    (DateTime.Today -
                        x.LastNdtDate.Value)
                    .TotalDays > 7);

            metrics.RepairRate =
                metrics.TotalWelds == 0
                    ? 0
                    : Math.Round(
                        (double)welds.Count(
                            x => x.RepairCount > 0)
                        /
                        metrics.TotalWelds
                        * 100,
                        2);

            metrics.FirstPassYield =
                metrics.TotalWelds == 0
                    ? 0
                    : Math.Round(
                        (double)welds.Count(
                            x =>
                                x.Status ==
                                WeldStatusType.Accepted
                                &&
                                x.RepairCount == 0)
                        /
                        metrics.TotalWelds
                        * 100,
                        2);

            metrics.ReleaseRate =
                metrics.TotalWelds == 0
                    ? 0
                    : Math.Round(
                        (double)metrics.ReleasedCount
                        /
                        metrics.TotalWelds
                        * 100,
                        2);

            metrics.FilteredWelds =
                welds
                    .Select(x =>
                        new ControlTowerWeldItem
                        {
                            WeldId = x.Id,
                            WeldNumber = x.WeldNumber,
                            WorkflowStatus =
                                x.WorkflowStatus.ToString(),
                            WpsNumber =
                                x.WpsNumber,
                            WelderNumber =
                                x.WelderNumber,
                            Summary =
                                x.ReadinessSummary
                        })
                    .ToList();

            metrics.DeadlineRisks =
    welds
        .Where(x =>
            x.WorkflowStatus ==
            WeldWorkflowStatus.NdtPending &&
            x.NdtPendingDate.HasValue)
        .Select(x =>
        {
            if (!x.NdtPendingDate.HasValue)
                return null;

            var days =
                (DateTime.Today -
                 x.NdtPendingDate.Value.Date)
                .Days;

            return new DeadlineRiskItem
            {
                WeldId = x.Id,
                WeldNumber = x.WeldNumber,
                DaysWaiting = days,
                RiskLevel =
                    days > 14
                        ? "High"
                        : days > 7
                            ? "Medium"
                            : "Low",
                Reason =
                    $"Waiting {days} days for NDT."
            };
        })
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();

            if (metrics.BlockedCount > 0)
            {
                metrics.Alerts.Add(
                    new ControlTowerAlert
                    {
                        Severity = "High",
                        Message =
                            $"{metrics.BlockedCount} welds are blocked."
                    });
            }

            if (metrics.OverdueNdtCount > 0)
            {
                metrics.Alerts.Add(
                    new ControlTowerAlert
                    {
                        Severity = "Medium",
                        Message =
                            $"{metrics.OverdueNdtCount} welds have overdue NDT."
                    });
            }

            if (metrics.RepairRequiredCount > 0)
            {
                metrics.Alerts.Add(
                    new ControlTowerAlert
                    {
                        Severity = "Medium",
                        Message =
                            $"{metrics.RepairRequiredCount} welds require repair."
                    });
            }

            return metrics;
        }
    }
}
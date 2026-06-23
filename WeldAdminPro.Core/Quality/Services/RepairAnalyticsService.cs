using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class RepairAnalyticsService
    {
        public RepairAnalytics Generate(
            List<RepairRecord> repairs)
        {
            var analytics =
                new RepairAnalytics();

            analytics.TotalRepairs =
                repairs.Count;

            analytics.OpenRepairs =
                repairs.Count(x =>
                    x.Status != RepairStatus.Closed);

            analytics.ClosedRepairs =
                repairs.Count(x =>
                    x.Status == RepairStatus.Closed);

            // =========================
            // AVG CLOSURE DAYS
            // =========================

            var completed =
                repairs
                .Where(x =>
                    x.CompletedDate != null)
                .ToList();

            if (completed.Any())
            {
                analytics.AverageClosureDays =
                    completed.Average(x =>
                        (x.CompletedDate!.Value
                        - x.RequestedDate)
                        .TotalDays);
            }

            // =========================
            // WORST WELDER
            // =========================

            analytics.WorstWelder =
                repairs
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.RepairedByWelder))
                .GroupBy(x =>
                    x.RepairedByWelder)
                .OrderByDescending(x =>
                    x.Count())
                .FirstOrDefault()
                ?.Key
                ?? "N/A";

            // =========================
            // WORST WPS
            // =========================

            analytics.WorstWps =
                repairs
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.RepairWpsNumber))
                .GroupBy(x =>
                    x.RepairWpsNumber)
                .OrderByDescending(x =>
                    x.Count())
                .FirstOrDefault()
                ?.Key
                ?? "N/A";

            return analytics;
        }
    }
}
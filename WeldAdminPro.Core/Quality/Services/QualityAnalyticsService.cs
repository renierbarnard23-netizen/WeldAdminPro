using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class QualityAnalyticsService
    {
        public QualityKpiModel Generate(
            List<Weld> welds,
            List<NcrRecord> ncrs,
            List<CapaRecord> capas)
        {
            var model =
                new QualityKpiModel();

            // =====================================
            // BASIC COUNTS
            // =====================================

            model.TotalWelds =
                welds.Count;

            model.RejectedWelds =
                welds.Count(x =>
                    x.RequiresRepair);

            model.TotalNcrs =
                ncrs.Count;

            model.OpenNcrs =
                ncrs.Count(x =>
                    !x.IsClosed);

            model.TotalCapas =
                capas.Count;

            model.OpenCapas =
                capas.Count(x =>
                    x.Status !=
                    Enums.CapaStatus.Closed);

            // =====================================
            // RATES
            // =====================================

            model.RejectRate =
                model.TotalWelds == 0
                    ? 0
                    : ((double)
                        model.RejectedWelds
                        / model.TotalWelds) * 100;

            model.RepairRate =
                model.TotalWelds == 0
                    ? 0
                    : ((double)
                        welds.Count(x =>
                            x.RequiresRepair)
                        / model.TotalWelds) * 100;

            // =====================================
            // OVERDUE CAPAS
            // =====================================

            model.OverdueCapas =
                capas.Count(x =>
                    x.DueDate < DateTime.UtcNow
                    &&
                    x.Status !=
                    Enums.CapaStatus.Closed);

            // =====================================
            // WORST WELDER
            // =====================================

            var worstWelder =
                welds
                .Where(x => x.RequiresRepair)
                .GroupBy(x => x.WelderNumber)
                .OrderByDescending(x => x.Count())
                .FirstOrDefault();

            model.WorstWelder =
                worstWelder == null
                    ? "N/A"
                    : worstWelder.Key;

            // =====================================
            // TOP DEFECT TYPE
            // =====================================

            model.TopDefectType =
                "N/A";

            // =====================================
            // QUALITY SCORE
            // =====================================

            model.QualityScore =
                100
                - model.RejectRate
                - (model.OpenNcrs * 2)
                - (model.OverdueCapas * 3);

            // =====================================
            // RISK LEVEL
            // =====================================

            if (model.QualityScore >= 90)
            {
                model.RiskLevel = "LOW";
            }
            else if (model.QualityScore >= 75)
            {
                model.RiskLevel = "MEDIUM";
            }
            else
            {
                model.RiskLevel = "HIGH";
            }

            // =====================================
            // ALERTS
            // =====================================

            if (model.OpenNcrs > 5)
            {
                model.Alerts.Add(
                    "High number of open NCRs.");
            }

            if (model.OverdueCapas > 3)
            {
                model.Alerts.Add(
                    "Multiple overdue CAPAs detected.");
            }

            if (model.RejectRate > 10)
            {
                model.Alerts.Add(
                    "Reject rate exceeds 10%.");
            }

            // =====================================
            // WELDER PERFORMANCE
            // =====================================

            var welderGroups =
                welds.GroupBy(x =>
                    x.WelderNumber);

            foreach (var group in welderGroups)
            {
                var total =
                    group.Count();

                var repairs =
                    group.Count(x =>
                        x.RequiresRepair);

                var rate =
                    total == 0
                        ? 0
                        : ((double)repairs / total) * 100;

                var risk =
                    rate > 15
                        ? "HIGH"
                        : rate > 5
                            ? "MEDIUM"
                            : "LOW";

                model.WelderPerformance.Add(
                    new WelderPerformanceModel
                    {
                        WelderNumber =
                            group.Key,

                        TotalWelds =
                            total,

                        Repairs =
                            repairs,

                        RepairRate =
                            rate,

                        RiskLevel =
                            risk
                    });
            }

            // =====================================
            // DEFECT PARETO
            // =====================================

            var defectGroups =
                welds
                .Where(x => x.RequiresRepair)
                .GroupBy(x => x.DefectType);

            var totalDefects =
                defectGroups.Sum(x => x.Count());

            double cumulative = 0;

            foreach (var defect in defectGroups
                .OrderByDescending(x => x.Count()))
            {
                var percentage =
                    totalDefects == 0
                        ? 0
                        : (double)defect.Count()
                            / totalDefects * 100;

                cumulative += percentage;

                model.ParetoDefects.Add(
                    new DefectParetoModel
                    {
                        Defect =
                            defect.Key,

                        Count =
                            defect.Count(),

                        Percentage =
                            percentage,

                        CumulativePercentage =
                            cumulative
                    });
            }

            // =====================================
            // CAPA AGING
            // =====================================

            foreach (var capa in capas)
            {
                var age =
                    (DateTime.UtcNow - capa.CreatedDate)
                    .Days;

                var remaining =
                    (capa.DueDate - DateTime.UtcNow)
                    .Days;

                var risk =
                    remaining < 0
                        ? "HIGH"
                        : remaining < 7
                            ? "MEDIUM"
                            : "LOW";

                model.CapaAging.Add(
                    new CapaAgingModel
                    {
                        Title =
                            capa.Title,

                        AssignedTo =
                            capa.CreatedBy,

                        AgeDays =
                            age,

                        DaysRemaining =
                            remaining,

                        Status =
                            capa.Status.ToString(),

                        RiskLevel =
                            risk
                    });
            }

            // =====================================
            // NCR LIFECYCLE
            // =====================================

            foreach (var ncr in ncrs)
            {
                var age =
                    (DateTime.UtcNow - ncr.RaisedDate)
                    .Days;

                var overdue =
                    !ncr.IsClosed
                    && age > 14;

                var risk =
                    overdue
                        ? "HIGH"
                        : age > 7
                            ? "MEDIUM"
                            : "LOW";

                model.NcrLifecycle.Add(
                    new NcrLifecycleModel
                    {
                        NcrNumber =
                            ncr.NcrNumber,

                        Status =
                            ncr.Status.ToString(),

                        AgeDays =
                            age,

                        IsOverdue =
                            overdue,

                        RiskLevel =
                            risk
                    });
            }

            // =====================================
            // REPAIR COST INTELLIGENCE
            // =====================================

            var repairGroups =
                welds
                .Where(x => x.RepairCycle > 0)
                .GroupBy(x => x.WelderNumber);

            foreach (var group in repairGroups)
            {
                var repairs =
                    group.Count();

                var hours =
                    repairs * 3.5;

                var cost =
                    hours * 850;

                var risk =
                    cost > 50000
                        ? "HIGH"
                        : cost > 15000
                            ? "MEDIUM"
                            : "LOW";

                model.RepairCosts.Add(
                    new RepairCostModel
                    {
                        WelderNumber =
                            group.Key,

                        TotalRepairs =
                            repairs,

                        EstimatedRepairHours =
                            hours,

                        EstimatedRepairCost =
                            cost,

                        RiskLevel =
                            risk
                    });
            }

            // =====================================
            // WELDER QUALIFICATION RISK
            // =====================================

            var qualificationGroups =
                welds
                .GroupBy(x => x.WelderNumber);

            foreach (var group in qualificationGroups)
            {
                var latestWeld =
                    group
                    .OrderByDescending(x => x.CreatedDate)
                    .First();

                var expiryDate =
                    latestWeld.CreatedDate.AddMonths(6);

                var remaining =
                    (expiryDate - DateTime.UtcNow)
                    .Days;

                var expired =
                    remaining < 0;

                var risk =
                    expired
                        ? "HIGH"
                        : remaining < 30
                            ? "MEDIUM"
                            : "LOW";

                model.QualificationRisks.Add(
                    new WelderQualificationRiskModel
                    {
                        WelderNumber =
                            latestWeld.WelderNumber,

                        Process =
                            latestWeld.Process ?? "N/A",

                        MaterialGroup =
                            latestWeld.MaterialGroup ?? "N/A",

                        DaysRemaining =
                            remaining,

                        IsExpired =
                            expired,

                        RiskLevel =
                            risk
                    });
            }

            // =====================================
            // PRODUCTION BOTTLENECK ANALYTICS
            // =====================================

            // REPAIRS

            var repairCount =
                welds.Count(x =>
                    x.RequiresRepair);

            model.Bottlenecks.Add(
                new ProductionBottleneckModel
                {
                    Category =
                        "Repair Queue",

                    Count =
                        repairCount,

                    RiskLevel =
                        repairCount > 25
                            ? "HIGH"
                            : repairCount > 10
                                ? "MEDIUM"
                                : "LOW",

                    Recommendation =
                        repairCount > 25
                            ? "Increase repair welding capacity."
                            : "Monitor repair throughput."
                });

            // NCRs

            model.Bottlenecks.Add(
                new ProductionBottleneckModel
                {
                    Category =
                        "Open NCRs",

                    Count =
                        model.OpenNcrs,

                    RiskLevel =
                        model.OpenNcrs > 15
                            ? "HIGH"
                            : model.OpenNcrs > 5
                                ? "MEDIUM"
                                : "LOW",

                    Recommendation =
                        model.OpenNcrs > 15
                            ? "Immediate QA escalation required."
                            : "Maintain NCR monitoring."
                });

            // OVERDUE CAPAS

            model.Bottlenecks.Add(
                new ProductionBottleneckModel
                {
                    Category =
                        "Overdue CAPAs",

                    Count =
                        model.OverdueCapas,

                    RiskLevel =
                        model.OverdueCapas > 5
                            ? "HIGH"
                            : model.OverdueCapas > 2
                                ? "MEDIUM"
                                : "LOW",

                    Recommendation =
                        model.OverdueCapas > 5
                            ? "Management intervention required."
                            : "Continue CAPA follow-up."
                });

            // =====================================
            // AI REPAIR PREDICTION ENGINE
            // =====================================

            foreach (var group in welds
                .GroupBy(x => x.WelderNumber))
            {
                var total =
                    group.Count();

                var repairs =
                    group.Count(x =>
                        x.RequiresRepair);

                var rate =
                    total == 0
                        ? 0
                        : ((double)repairs / total) * 100;

                string prediction;
                string risk;
                string recommendation;

                if (rate >= 20)
                {
                    prediction =
                        "High probability of future repairs";

                    risk =
                        "HIGH";

                    recommendation =
                        "Immediate welder review and retraining.";
                }
                else if (rate >= 10)
                {
                    prediction =
                        "Moderate repair trend detected";

                    risk =
                        "MEDIUM";

                    recommendation =
                        "Increase QA inspection frequency.";
                }
                else
                {
                    prediction =
                        "Stable welding performance";

                    risk =
                        "LOW";

                    recommendation =
                        "Continue standard monitoring.";
                }

                model.RepairPredictions.Add(
                    new RepairPredictionModel
                    {
                        WelderNumber =
                            group.Key,

                        TotalWelds =
                            total,

                        Repairs =
                            repairs,

                        RepairRate =
                            rate,

                        Prediction =
                            prediction,

                        RiskLevel =
                            risk,

                        Recommendation =
                            recommendation
                    });
            }

            return model;
        }
    }
}

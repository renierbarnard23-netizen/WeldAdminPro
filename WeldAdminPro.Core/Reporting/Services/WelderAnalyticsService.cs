using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class WelderAnalyticsService
    {
        public List<WelderPerformanceMetrics>
            BuildWelderMetrics(
                List<Weld> welds)
        {
            var result =
                new List<WelderPerformanceMetrics>();

            var grouped =
                welds.GroupBy(x => x.WelderNumber);

            foreach (var group in grouped)
            {
                var total =
                    group.Count();

                var accepted =
                    group.Count(x =>
                        x.Status ==
                        WeldStatusType.Accepted ||
                        x.Status ==
                        WeldStatusType.Closed);

                var repairs =
                    group.Count(x =>
                        x.RepairCycle > 0);

                var rejected =
                    group.Count(x =>
                        x.Status ==
                        WeldStatusType.RepairRequired);

                var repeatRepairs =
                    group.Count(x =>
                        x.RepairCycle > 1);

                var metrics =
                    new WelderPerformanceMetrics
                    {
                        WelderNumber =
                            group.Key,

                        TotalWelds =
                            total,

                        AcceptedWelds =
                            accepted,

                        Repairs =
                            repairs,

                        RejectedWelds =
                            rejected,

                        RepeatRepairs =
                            repeatRepairs,

                        RepairRate =
                            total == 0
                                ? 0
                                : Math.Round(
                                    (double)repairs /
                                    total * 100,
                                    1),

                        AcceptanceRate =
                            total == 0
                                ? 0
                                : Math.Round(
                                    (double)accepted /
                                    total * 100,
                                    1)
                    };

                result.Add(metrics);
            }

            return result
                .OrderByDescending(x => x.RepairRate)
                .ToList();
        }
    }
}
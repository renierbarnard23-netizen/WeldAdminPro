using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class WelderPerformanceService
    {
        public List<WelderPerformanceModel>
            Build(
                List<Weld> welds)
        {
            var results =
                new List<WelderPerformanceModel>();

            var groups =
                welds.GroupBy(x =>
                    x.WelderNumber);

            foreach (var group in groups)
            {
                var total =
                    group.Count();

                var accepted =
                    group.Count(x =>
                        (x.Status ==
                            WeldStatusType.Accepted
                        ||
                        x.Status ==
                            WeldStatusType.Closed)
                        &&
                        x.RepairCycle == 0);

                var repairs =
                    group.Count(x =>
                        x.RepairCycle > 0);

                var repeatRepairs =
                    group.Count(x =>
                        x.RepairCycle > 1);

                var worstWps =
                    group.GroupBy(x =>
                        x.WpsNumber)
                    .OrderByDescending(x =>
                        x.Count(y =>
                            y.RepairCycle > 0))
                    .FirstOrDefault()
                    ?.Key ?? "";

                results.Add(
                    new WelderPerformanceModel
                    {
                        WelderNumber =
                            group.Key,

                        TotalWelds =
                            total,

                        AcceptedWelds =
                            accepted,

                        Repairs =
                            repairs,

                        RepeatRepairs =
                            repeatRepairs,

                        RepairRate =
                            total == 0
                                ? 0
                                : repairs * 100.0 / total,

                        WorstWps =
                            worstWps
                    });
            }

            return results
                .OrderByDescending(x =>
                    x.RepairRate)
                .ToList();
        }
    }
}

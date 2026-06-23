using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldAnalyticsService
    {
        public WeldRepairAnalytics Generate(
            List<Weld> welds)
        {
            var analytics =
                new WeldRepairAnalytics();

            analytics.TotalWelds =
                welds.Count;

            analytics.TotalRepairs =
                welds.Count(x =>
                    x.RepairCycle > 0);

            analytics.RepairRate =
                analytics.TotalWelds == 0
                ? 0
                : (double)analytics.TotalRepairs
                  / analytics.TotalWelds * 100;

            analytics.RepeatRepairWelds =
                welds.Count(x =>
                    x.RepairCycle > 1);

            analytics.MostFailedWelder =
                welds
                    .Where(x => x.RepairCycle > 0)
                    .GroupBy(x => x.WelderNumber)
                    .OrderByDescending(x => x.Count())
                    .FirstOrDefault()
                    ?.Key ?? "N/A";

            analytics.MostFailedWps =
                welds
                    .Where(x => x.RepairCycle > 0)
                    .GroupBy(x => x.WpsNumber)
                    .OrderByDescending(x => x.Count())
                    .FirstOrDefault()
                    ?.Key ?? "N/A";

            return analytics;
        }
    }
}
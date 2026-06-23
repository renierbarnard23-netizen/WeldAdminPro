using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Models;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Analytics.Services
{
    public class WelderAnalyticsService
    {
        public List<WelderPerformanceRecord>
            Generate(
                IEnumerable<Weld> welds,
                IEnumerable<NcrRecord> ncrs)
        {
            var result =
                new List<WelderPerformanceRecord>();

            var grouped =
                welds.GroupBy(x =>
                    x.WelderNumber);

            foreach (var group in grouped)
            {
                var welderWelds =
                    group.ToList();

                var rejected =
                    welderWelds.Count(x =>
                        x.RequiresRepair);

                var repairCount =
                    welderWelds.Sum(x =>
                        x.RepairCount);

                var weldIds =
                    welderWelds
                        .Select(x => x.Id)
                        .ToList();

                var ncrCount =
                    ncrs.Count(x =>
                        weldIds.Contains(
                            x.WeldId));

                result.Add(
                    new WelderPerformanceRecord
                    {
                        WelderNumber =
                            group.Key,

                        TotalWelds =
                            welderWelds.Count,

                        RejectedWelds =
                            rejected,

                        RepairCount =
                            repairCount,

                        RejectRate =
                            welderWelds.Count == 0
                                ? 0
                                : ((double)rejected
                                    / welderWelds.Count) * 100,

                        NcrCount =
                            ncrCount
                    });
            }

            return result
                .OrderByDescending(x =>
                    x.RejectRate)
                .ToList();
        }
    }
}
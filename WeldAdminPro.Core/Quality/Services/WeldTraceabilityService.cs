using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldTraceabilityService
    {
        public List<WeldTraceabilityRow> Build(
            List<Weld> welds,
            List<WeldNdtResult> ndtResults)
        {
            var rows =
                new List<WeldTraceabilityRow>();

            foreach (var weld in welds)
            {
                var weldNdt =
                    ndtResults
                    .Where(x => x.WeldId == weld.Id)
                    .ToList();

                var repairs =
                    weldNdt.Count();

                var latestNdt =
                    weldNdt.LastOrDefault();

                rows.Add(
                    new WeldTraceabilityRow
                    {
                        WeldNumber =
                            weld.WeldNumber,

                        Welder =
                            "Assigned",

                        WpsNumber =
                            weld.WpsNumber,

                        Material =
                            "Material Pending",

                        WeldStatus =
                            weld.Status.ToString(),

                        NdtStatus =
                            latestNdt != null
                                ? latestNdt.Result.ToString()
                                : "Pending",

                        RepairCount =
                            repairs
                    });
            }

            return rows;
        }
    }
}

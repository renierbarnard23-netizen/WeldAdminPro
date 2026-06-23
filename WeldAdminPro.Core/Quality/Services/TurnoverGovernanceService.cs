using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class TurnoverGovernanceService
    {
        public TurnoverReadinessRecord Analyze(
            Weld weld,
            bool ndtAccepted,
            bool noOpenRepairs,
            bool noOpenNcrs,
            bool holdPointsApproved,
            bool documentsAttached)
        {
            var reasons =
                new List<string>();

            if (!weld.ReleaseReady)
            {
                reasons.Add(
                    "Weld not released.");
            }

            if (!ndtAccepted)
            {
                reasons.Add(
                    "No accepted NDT.");
            }

            if (!noOpenRepairs)
            {
                reasons.Add(
                    "Open repairs exist.");
            }

            if (!noOpenNcrs)
            {
                reasons.Add(
                    "Open NCRs exist.");
            }

            if (!holdPointsApproved)
            {
                reasons.Add(
                    "Hold points pending.");
            }

            if (!documentsAttached)
            {
                reasons.Add(
                    "Documents missing.");
            }

            return new TurnoverReadinessRecord
            {
                WeldNumber =
                    weld.WeldNumber,

                Released =
                    weld.ReleaseReady,

                NdtAccepted =
                    ndtAccepted,

                NoOpenRepairs =
                    noOpenRepairs,

                NoOpenNcrs =
                    noOpenNcrs,

                HoldPointsApproved =
                    holdPointsApproved,

                DocumentsAttached =
                    documentsAttached,

                TurnoverReady =
                    !reasons.Any(),

                BlockingReasons =
                    string.Join(
                        "\n",
                        reasons)
            };
        }
    }
}
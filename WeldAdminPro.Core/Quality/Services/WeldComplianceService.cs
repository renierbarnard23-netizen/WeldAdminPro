using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldComplianceService
    {
        public void ApplyNdtResult(
            Weld weld,
            WeldNdtResult ndt)
        {
            weld.LastNdtDate =
                ndt.InspectionDate;

            weld.LastNdtResult =
                ndt.Result.ToString();

            // =====================================
            // REPAIR REQUIRED
            // =====================================

            if (ndt.RequiresRepair)
            {
                weld.Status =
                    WeldStatusType.RepairRequired;

                weld.NdtStatus =
                    "Failed";

                weld.RequiresRepair = true;

                weld.RepairCycle += 1;

                if (weld.RepairCycle >= 3)
                {
                    weld.Status =
                        WeldStatusType.Rejected;

                    weld.NdtStatus =
                        "Permanently Rejected";

                    return;
                }

                return;
            }

            // =====================================
            // ACCEPTED
            // =====================================

            if (ndt.Result ==
                NdtResultType.Accept)
            {
                weld.Status =
                    WeldStatusType.Accepted;

                weld.NdtStatus =
                    "Passed";

                weld.RequiresRepair = false;
            }
            else
            {
                weld.Status =
                    WeldStatusType.Rejected;

                weld.NdtStatus =
                    "Failed";

                weld.RequiresRepair = true;
            }
        }

        // =====================================
        // MARK REPAIRED
        // =====================================

        public void MarkRepaired(
            Weld weld)
        {
            weld.Status =
                WeldStatusType.AwaitingReinspection;

            weld.NdtStatus =
                "Pending Reinspection";

            weld.RequiresRepair = false;

            weld.RepairCount += 1;
        }
    }
}


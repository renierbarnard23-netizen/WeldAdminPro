using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldLifecycleRuleService
    {
        public bool CanAddNdt(
            Weld weld,
            out string error)
        {
            error = "";

            if (weld.Status ==
                WeldStatusType.Pending)
            {
                error =
                    "Cannot add NDT to pending weld.";

                return false;
            }

            if (weld.Status ==
                WeldStatusType.Closed)
            {
                error =
                    "Cannot add NDT to closed weld.";

                return false;
            }

            return true;
        }

        public bool CanRepair(
            Weld weld,
            out string error)
        {
            error = "";

            if (weld.Status !=
                WeldStatusType.RepairRequired)
            {
                error =
                    "Only repair required welds can be repaired.";

                return false;
            }

            return true;
        }

        public bool CanReinspect(
            Weld weld,
            out string error)
        {
            error = "";

            if (weld.Status !=
                WeldStatusType.AwaitingReinspection)
            {
                error =
                    "Weld is not awaiting reinspection.";

                return false;
            }

            return true;
        }

        public bool CanClose(
            Weld weld,
            out string error)
        {
            error = "";

            if (weld.Status !=
                WeldStatusType.Accepted)
            {
                error =
                    "Only accepted welds can be closed.";

                return false;
            }

            return true;
        }
    }
}
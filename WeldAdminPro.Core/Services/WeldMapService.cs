using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Services
{
    public class WeldMapService
    {
        public string GetStatusColor(
            Weld weld)
        {
            if (weld.RequiresRepair)
            {
                return "Red";
            }

            if (weld.IsReleased)
            {
                return "LightGreen";
            }

            if (weld.WorkflowStatus
                == WeldWorkflowStatus.NdtPending)
            {
                return "Khaki";
            }

            return "LightGray";
        }
    }
}
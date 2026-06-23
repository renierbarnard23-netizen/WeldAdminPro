using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Services
{
    public static class NcrWorkflowService
    {
        public static bool CanMoveTo(
            NcrStatus current,
            NcrStatus target)
        {
            return current switch
            {
                NcrStatus.Open =>
                    target ==
                    NcrStatus.UnderInvestigation,

                NcrStatus.UnderInvestigation =>
                    target ==
                    NcrStatus.AwaitingDisposition,

                NcrStatus.AwaitingDisposition =>
                    target ==
                    NcrStatus.ApprovedForRepair
                    || target ==
                    NcrStatus.Rejected,

                NcrStatus.ApprovedForRepair =>
                    target ==
                    NcrStatus.RepairInProgress,

                NcrStatus.RepairInProgress =>
                    target ==
                    NcrStatus.PendingVerification,

                NcrStatus.PendingVerification =>
                    target ==
                    NcrStatus.Closed,

                _ => false
            };
        }
    }
}
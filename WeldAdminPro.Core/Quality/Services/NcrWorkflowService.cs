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
                    NcrStatus.PendingVerification
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

        public static NcrStatus?
            GetPostDispositionStatus(
                NcrDispositionType disposition)
        {
            return disposition switch
            {
                NcrDispositionType.Repair =>
                    NcrStatus.ApprovedForRepair,

                NcrDispositionType.Rework =>
                    NcrStatus.ApprovedForRepair,

                NcrDispositionType.UseAsIs =>
                    NcrStatus.PendingVerification,

                NcrDispositionType.Scrap =>
                    NcrStatus.PendingVerification,

                NcrDispositionType.EngineeringReview =>
                    null,

                _ =>
                    null
            };
        }

        public static bool
            RequiresExecution(
                NcrDispositionType disposition)
        {
            return disposition ==
                   NcrDispositionType.Repair
                   ||
                   disposition ==
                   NcrDispositionType.Rework;
        }

        public static bool
            RequiresFinalVerification(
                NcrDispositionType disposition)
        {
            return disposition switch
            {
                NcrDispositionType.Repair => true,
                NcrDispositionType.Rework => true,
                NcrDispositionType.UseAsIs => true,
                NcrDispositionType.Scrap => true,
                NcrDispositionType.EngineeringReview => false,
                _ => false
            };
        }
    }
}

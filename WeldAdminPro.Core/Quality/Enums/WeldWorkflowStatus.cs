namespace WeldAdminPro.Core.Quality.Enums
{
    public enum WeldWorkflowStatus
    {
        Draft = 0,

    ReadyForFitUp = 1,

        FitUpComplete = 2,

        ReadyForWelding = 3,

        Welded = 4,

        VisualInspectionPending = 5,

        NdtPending = 6,

        NdtInProgress = 7,

        Accepted = 8,

        RepairRequired = 9,

        UnderRepair = 10,

        ReinspectionRequired = 11,

        Released = 12,

        TurnoverReady = 13,

        Closed = 14
    }

}

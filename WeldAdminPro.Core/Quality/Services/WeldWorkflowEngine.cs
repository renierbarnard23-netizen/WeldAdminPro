using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldWorkflowEngine
    {
        private readonly WeldTransitionRuleEngine
            _ruleEngine;

        public WeldWorkflowEngine()
        {
            _ruleEngine =
                new WeldTransitionRuleEngine();
        }

        public bool CanTransition(
            WeldWorkflowStatus current,
            WeldWorkflowStatus next)
        {
            return (current, next) switch
            {
                (WeldWorkflowStatus.Draft,
                    WeldWorkflowStatus.ReadyForFitUp) => true,

                (WeldWorkflowStatus.ReadyForFitUp,
                    WeldWorkflowStatus.FitUpComplete) => true,

                (WeldWorkflowStatus.FitUpComplete,
                    WeldWorkflowStatus.ReadyForWelding) => true,

                (WeldWorkflowStatus.ReadyForWelding,
                    WeldWorkflowStatus.Welded) => true,

                (WeldWorkflowStatus.Welded,
                    WeldWorkflowStatus.VisualInspectionPending) => true,

                (WeldWorkflowStatus.VisualInspectionPending,
                    WeldWorkflowStatus.NdtPending) => true,

                (WeldWorkflowStatus.NdtPending,
                    WeldWorkflowStatus.NdtInProgress) => true,

                (WeldWorkflowStatus.NdtInProgress,
                    WeldWorkflowStatus.Accepted) => true,

                (WeldWorkflowStatus.NdtInProgress,
                    WeldWorkflowStatus.RepairRequired) => true,

                (WeldWorkflowStatus.RepairRequired,
                    WeldWorkflowStatus.UnderRepair) => true,

                (WeldWorkflowStatus.UnderRepair,
                    WeldWorkflowStatus.ReinspectionRequired) => true,

                (WeldWorkflowStatus.ReinspectionRequired,
                    WeldWorkflowStatus.NdtPending) => true,

                (WeldWorkflowStatus.Accepted,
                    WeldWorkflowStatus.Released) => true,

                (WeldWorkflowStatus.Released,
                    WeldWorkflowStatus.TurnoverReady) => true,

                (WeldWorkflowStatus.TurnoverReady,
                    WeldWorkflowStatus.Closed) => true,

                _ => false
            };
        }

        /// <summary>
        /// Returns the structurally allowed next workflow
        /// states for the supplied current status.
        ///
        /// The transition map remains authoritative in
        /// CanTransition. This method does not duplicate
        /// lifecycle rules.
        /// </summary>
        public IReadOnlyList<WeldWorkflowStatus>
            GetAllowedTransitions(
            WeldWorkflowStatus currentStatus)
        {
            return Enum
                .GetValues<WeldWorkflowStatus>()
                .Where(targetStatus =>
                    CanTransition(
                        currentStatus,
                        targetStatus))
                .ToList();
        }

        public WeldStateTransitionResult
            TryTransition(
            Weld weld,
            WeldWorkflowStatus targetStatus,
            bool hasApprovedWps = true,
            bool hasValidWelder = true,
            bool hasAcceptedNdt = true,
            bool hasOpenRepairs = false)
        {
            var result =
                new WeldStateTransitionResult();

            // =========================
            // BASIC WORKFLOW VALIDATION
            // =========================

            if (!CanTransition(
                    weld.WorkflowStatus,
                    targetStatus))
            {
                result.Success = false;

                result.ErrorMessage =
                    $"Invalid workflow transition from " +
                    $"{weld.WorkflowStatus} to " +
                    $"{targetStatus}.";

                return result;
            }

            // =========================
            // RULE ENGINE VALIDATION
            // =========================

            var ruleResult =
                _ruleEngine.CanTransition(
                    weld,
                    targetStatus,
                    hasApprovedWps,
                    hasValidWelder,
                    hasAcceptedNdt,
                    hasOpenRepairs);

            if (!ruleResult.Success)
            {
                return ruleResult;
            }

            // =========================
            // APPLY TRANSITION
            // =========================

            weld.WorkflowStatus =
                targetStatus;

            result.Success = true;

            return result;
        }

        public bool MoveToRepair(
            Weld weld,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.RepairRequired);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool MarkUnderRepair(
            Weld weld,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.UnderRepair);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool MarkReinspectionRequired(
            Weld weld,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.ReinspectionRequired);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool MarkAccepted(
            Weld weld,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.Accepted);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool ReleaseWeld(
            Weld weld,
            bool hasApprovedWps,
            bool hasValidWelder,
            bool hasAcceptedNdt,
            bool hasOpenRepairs,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.Released,
                    hasApprovedWps,
                    hasValidWelder,
                    hasAcceptedNdt,
                    hasOpenRepairs);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool MarkTurnoverReady(
            Weld weld,
            bool hasAcceptedNdt,
            bool hasOpenRepairs,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.TurnoverReady,
                    true,
                    true,
                    hasAcceptedNdt,
                    hasOpenRepairs);

            error =
                result.ErrorMessage;

            return result.Success;
        }

        public bool CloseWeld(
            Weld weld,
            out string error)
        {
            var result =
                TryTransition(
                    weld,
                    WeldWorkflowStatus.Closed);

            error =
                result.ErrorMessage;

            return result.Success;
        }
    }
}

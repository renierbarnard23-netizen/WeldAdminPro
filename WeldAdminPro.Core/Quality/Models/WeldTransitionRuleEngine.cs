using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldTransitionRuleEngine
    {
        public WeldStateTransitionResult
            CanTransition(
            Weld weld,
            WeldWorkflowStatus targetStatus,
            bool hasApprovedWps,
            bool hasValidWelder,
            bool hasAcceptedNdt,
            bool hasOpenRepairs)
        {
            var result =
                new WeldStateTransitionResult
                {
                    Success = true
                };

            // =========================
            // RELEASE RULES
            // =========================

            if (targetStatus ==
                WeldWorkflowStatus.Released)
            {
                if (!hasApprovedWps)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "No approved WPS linked.");
                }

                if (!hasValidWelder)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "Welder qualification invalid.");
                }

                if (!hasAcceptedNdt)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "Accepted NDT result required.");
                }

                if (hasOpenRepairs)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "Open repairs exist.");
                }
            }

            // =========================
            // TURNOVER READY RULES
            // =========================

            if (targetStatus ==
                WeldWorkflowStatus.TurnoverReady)
            {
                if (!hasAcceptedNdt)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "Final accepted NDT required.");
                }

                if (hasOpenRepairs)
                {
                    result.Success = false;

                    result.BlockingIssues.Add(
                        "Repairs must be closed.");
                }
            }

            result.ErrorMessage =
                string.Join(
                    "\n",
                    result.BlockingIssues);

            return result;
        }
    }
}

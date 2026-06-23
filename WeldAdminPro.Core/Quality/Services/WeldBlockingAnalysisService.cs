using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldBlockingAnalysisService
    {
        public WeldBlockingResult Analyze(
            Weld weld,
            bool hasApprovedWps,
            bool hasQualifiedWelder,
            bool hasAcceptedNdt,
            bool hasOpenRepairs,
            bool turnoverDocumentsReady)
        {
            var result =
                new WeldBlockingResult
                {
                    CanRelease = true,
                    CanTurnover = true,
                    CanClose = true
                };

            // =========================
            // RELEASE CHECKS
            // =========================

            if (!hasApprovedWps)
            {
                result.CanRelease = false;

                result.BlockingReasons.Add(
                    "No approved WPS assigned.");
            }

            if (!hasQualifiedWelder)
            {
                result.CanRelease = false;

                result.BlockingReasons.Add(
                    "Welder qualification invalid or expired.");
            }

            if (!hasAcceptedNdt)
            {
                result.CanRelease = false;

                result.BlockingReasons.Add(
                    "No accepted NDT result.");
            }

            if (hasOpenRepairs)
            {
                result.CanRelease = false;

                result.BlockingReasons.Add(
                    "Open repairs still exist.");
            }

            // =========================
            // TURNOVER CHECKS
            // =========================

            if (!turnoverDocumentsReady)
            {
                result.CanTurnover = false;

                result.BlockingReasons.Add(
                    "Turnover documents incomplete.");
            }

            if (weld.WorkflowStatus
                != WeldWorkflowStatus.Released)
            {
                result.CanTurnover = false;

                result.BlockingReasons.Add(
                    "Weld has not been released.");
            }

            // =========================
            // CLOSE CHECKS
            // =========================

            if (weld.WorkflowStatus
                != WeldWorkflowStatus.TurnoverReady)
            {
                result.CanClose = false;

                result.BlockingReasons.Add(
                    "Weld is not turnover ready.");
            }

            return result;
        }
    }
}
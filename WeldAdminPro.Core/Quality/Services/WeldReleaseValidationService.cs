using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldReleaseValidationService
    {
        public WeldStateTransitionResult
            ValidateRelease(
            Weld weld,
            WeldReleaseContext context)
        {
            var result =
                new WeldStateTransitionResult
                {
                    Success = true
                };

            // =========================
            // WORKFLOW STATUS
            // =========================

            if (weld.WorkflowStatus !=
                WeldWorkflowStatus.Accepted)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Weld must be in Accepted status.");
            }

            // =========================
            // WPS VALIDATION
            // =========================

            if (!context.HasApprovedWps)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Approved WPS required.");
            }

            // =========================
            // WELDER VALIDATION
            // =========================

            if (!context.HasQualifiedWelder)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Welder qualification invalid.");
            }

            // =========================
            // NDT VALIDATION
            // =========================

            if (!context.HasAcceptedNdt)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Accepted NDT required.");
            }

            // =========================
            // REPAIR VALIDATION
            // =========================

            if (context.HasOpenRepairs)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Open repairs exist.");
            }

            // =========================
            // MATERIAL TRACEABILITY
            // =========================

            if (!context.HasMaterialTraceability)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Material traceability incomplete.");
            }

            // =========================
            // CONSUMABLE VALIDATION
            // =========================

            if (!context.HasValidConsumables)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Consumable traceability invalid.");
            }

            // =========================
            // CALIBRATION VALIDATION
            // =========================

            if (!context.HasCalibrationCompliance)
            {
                result.Success = false;

                result.BlockingIssues.Add(
                    "Calibration compliance failed.");
            }

            result.ErrorMessage =
                string.Join(
                    "\n",
                    result.BlockingIssues);

            return result;
        }
    }
}

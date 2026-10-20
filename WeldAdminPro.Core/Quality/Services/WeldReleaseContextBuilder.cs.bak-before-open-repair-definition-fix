using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldReleaseContextBuilder
    {
        public WeldReleaseContext Build(
            Weld weld,
            List<WeldNdtResult> ndtResults,
            List<RepairRecord> repairs,
            bool hasApprovedWps,
            bool hasQualifiedWelder)
        {
            var context =
                new WeldReleaseContext();

            // =========================
            // WPS VALIDATION
            // =========================

            context.HasApprovedWps =
                hasApprovedWps;

            // =========================
            // WELDER VALIDATION
            // =========================

            context.HasQualifiedWelder =
                hasQualifiedWelder;

            // =========================
            // ACCEPTED NDT
            // =========================

            context.HasAcceptedNdt =
                ndtResults.Any(x =>
                    x.Result ==
                    NdtResultType.Accept);

            // =========================
            // OPEN REPAIRS
            // =========================

            context.HasOpenRepairs =
                repairs.Any(x =>
                    x.Status !=
                    RepairStatus.Closed);

            // =========================
            // FUTURE MODULE PLACEHOLDERS
            // =========================

            context.HasMaterialTraceability =
                true;

            context.HasValidConsumables =
                true;

            context.HasCalibrationCompliance =
                true;

            return context;
        }
    }
}

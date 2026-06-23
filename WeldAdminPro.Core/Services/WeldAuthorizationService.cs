using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class WeldAuthorizationService
    {
        private readonly WelderQualificationService
            _welderService =
                new();

        private readonly WpsComplianceService
            _wpsService =
                new();

        public WeldAuthorizationResult Evaluate(
            Weld weld,
            Wps wps,
            Pqr pqr,

            string welderPosition,
            string welderPNumber,
            DateTime qualificationDate)
        {
            var result =
                new WeldAuthorizationResult();

            // =====================================
            // BASIC CHECKS
            // =====================================

            if (weld == null)
            {
                result.IsAuthorized = false;

                result.Reason =
                    "Weld missing.";

                return result;
            }

            if (wps == null)
            {
                result.IsAuthorized = false;

                result.Reason =
                    "No WPS assigned.";

                return result;
            }

            if (pqr == null)
            {
                result.IsAuthorized = false;

                result.Reason =
                    "No PQR linked.";

                return result;
            }

            // =====================================
            // WPS VALIDATION
            // =====================================

            var wpsResult =
                _wpsService.Evaluate(
                    wps,
                    pqr);

            if (!wpsResult.IsCompliant)
            {
                result.IsAuthorized = false;

                result.Reason =
                    string.Join(
                        Environment.NewLine,
                        wpsResult.AllIssues);

                return result;
            }

            // =====================================
            // WELDER VALIDATION
            // =====================================

            var welderResult =
                _welderService.Evaluate(
                    weld.WelderNumber,
                    wps.Process,
                    welderPosition,
                    welderPNumber,
                    qualificationDate,

                    wps.Process,
                    wps.Position ?? "",
                    wps.PNumber ?? "");

            if (!welderResult.IsQualified)
            {
                result.IsAuthorized = false;

                result.Reason =
                    welderResult.Reason;

                return result;
            }

            // =====================================
            // PASS
            // =====================================

            result.IsAuthorized = true;

            result.Reason =
                "Weld authorized.";

            return result;
        }
    }

    public class WeldAuthorizationResult
    {
        public bool IsAuthorized { get; set; }

        public string Reason { get; set; } = "";
    }
}
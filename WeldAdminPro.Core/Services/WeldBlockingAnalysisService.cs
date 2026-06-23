using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class WeldBlockingAnalysisService
    {
        private readonly WeldAuthorizationService
            _authorizationService =
                new();

        public WeldBlockingResult Analyze(
            Weld weld,
            Wps? wps,
            Pqr? pqr,

            string welderPosition,
            string welderPNumber,
            DateTime qualificationDate)
        {
            var result =
                new WeldBlockingResult();

            // =====================================
            // BASIC CHECKS
            // =====================================

            if (weld == null)
            {
                result.Blockers.Add(
                    "Weld record missing.");

                return result;
            }

            if (wps == null)
            {
                result.Blockers.Add(
                    "No WPS assigned.");

                return result;
            }

            if (pqr == null)
            {
                result.Blockers.Add(
                    "No linked PQR.");

                return result;
            }

            // =====================================
            // AUTHORIZATION
            // =====================================

            var authorization =
                _authorizationService.Evaluate(
                    weld,
                    wps,
                    pqr,

                    welderPosition,
                    welderPNumber,
                    qualificationDate);

            if (!authorization.IsAuthorized)
            {
                result.Blockers.Add(
                    authorization.Reason);
            }

            // =====================================
            // REPAIR CHECK
            // =====================================

            if (weld.RequiresRepair)
            {
                result.Blockers.Add(
                    "Open repair cycle exists.");
            }

            // =====================================
            // RELEASE STATUS
            // =====================================

            result.IsReady =
                result.Blockers.Count == 0;

            result.BlockingCount =
                result.Blockers.Count;

            result.Summary =
                result.IsReady
                    ? "READY FOR RELEASE"
                    : string.Join(
                        " | ",
                        result.Blockers);

            return result;
        }
    }

    public class WeldBlockingResult
    {
        public bool IsReady { get; set; }

        public int BlockingCount { get; set; }

        public string Summary { get; set; } = "";

        public List<string> Blockers { get; set; }
            = new();
    }
}
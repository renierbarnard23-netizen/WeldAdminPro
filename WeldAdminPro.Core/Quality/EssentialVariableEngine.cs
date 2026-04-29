using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class EssentialVariableEngine
    {
        public List<EssentialVariableResult> Evaluate(Wps wps, Pqr pqr)
        {
            var results = new List<EssentialVariableResult>();

            if (wps == null || pqr == null)
                return results;

            // =========================
            // QW-402 — JOINT
            // =========================
            if (!EqualsIgnoreCase(wps.JointType, pqr.JointType))
            {
                results.Add(Fail("QW-402", "Joint Type",
                    $"{wps.JointType} not qualified by {pqr.JointType}"));
            }

            if (!EqualsIgnoreCase(wps.JointDesign, pqr.JointDesign))
            {
                results.Add(Fail("QW-402", "Joint Design",
                    $"{wps.JointDesign} not qualified by {pqr.JointDesign}"));
            }

            // =========================
            // QW-403 — BASE METAL
            // =========================
            if (!EqualsIgnoreCase(wps.PNumber, pqr.PNumber))
            {
                results.Add(Fail("QW-403", "P-Number",
                    $"{wps.PNumber} not qualified by {pqr.PNumber}"));
            }

            // =========================
            // QW-404 — FILLER METAL
            // =========================
            if (!EqualsIgnoreCase(wps.FNumber, pqr.FNumber))
            {
                results.Add(Fail("QW-404", "F-Number",
                    $"{wps.FNumber} not qualified by {pqr.FNumber}"));
            }

            // =========================
            // QW-405 — POSITION
            // =========================
            if (!IsPositionQualified(wps.Position ?? "", pqr.QualifiedPosition ?? ""))
            {
                results.Add(Fail("QW-405", "Position",
                    $"{wps.Position} not qualified by {pqr.QualifiedPosition}"));
            }

            // =========================
            // QW-406 — PREHEAT
            // =========================
            if (wps.PreheatMin > 0 && pqr.Preheat > 0 && wps.PreheatMin < pqr.Preheat)
            {
                results.Add(Fail("QW-406", "Preheat",
                    $"WPS preheat {wps.PreheatMin} below qualified {pqr.Preheat}"));
            }

            // =========================
            // QW-409 — ELECTRICAL
            // =========================
            if (!EqualsIgnoreCase(wps.CurrentType, pqr.CurrentType))
            {
                results.Add(Fail("QW-409", "Current Type",
                    $"{wps.CurrentType} not qualified by {pqr.CurrentType}"));
            }

            // =========================
            // QW-410 — TECHNIQUE
            // =========================
            if (!EqualsIgnoreCase(wps.Progression, pqr.Progression))
            {
                results.Add(Fail("QW-410", "Progression",
                    $"{wps.Progression} not qualified by {pqr.Progression}"));
            }

            return results;
        }

        private EssentialVariableResult Fail(string code, string variable, string message)
        {
            return new EssentialVariableResult
            {
                Code = code,
                Variable = variable,
                Message = message,
                IsFailure = true
            };
        }

        private bool EqualsIgnoreCase(string? a, string? b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPositionQualified(string wpsPos, string pqrPos)
        {
            if (string.IsNullOrWhiteSpace(wpsPos) || string.IsNullOrWhiteSpace(pqrPos))
                return true;

            wpsPos = wpsPos.ToUpper();
            pqrPos = pqrPos.ToUpper();

            if (pqrPos == "6G") return true;
            if (pqrPos == "5G")
                return wpsPos is "1G" or "2G" or "5G";

            if (pqrPos == "3G")
                return wpsPos is "1G" or "2G" or "3G";

            return wpsPos == pqrPos;
        }
    }
}
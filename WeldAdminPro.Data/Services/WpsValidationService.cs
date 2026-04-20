using System;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
    public class WpsValidationService
    {
        public (bool IsValid, string Message) Validate(Wps wps, Pqr pqr)
        {
            if (wps == null)
                return (false, "No WPS provided");

            if (pqr == null)
                return (false, "No PQR linked");

            // =========================
            // 1. PROCESS
            // =========================
            if (!EqualsIgnoreCase(wps.Process, pqr.Process))
                return (false, $"Process mismatch: {wps.Process} vs {pqr.Process}");

            // =========================
            // 2. MATERIAL (P-NUMBER)
            // =========================
            if (!string.IsNullOrWhiteSpace(wps.PNumber) &&
                !EqualsIgnoreCase(wps.PNumber, pqr.PNumber))
                return (false, $"Material mismatch: {wps.PNumber} vs {pqr.PNumber}");

            // =========================
            // 3. THICKNESS RANGE
            // =========================
            double tolerance = 0.5;

            if (wps.ThicknessMin < (pqr.ThicknessQualifiedMin - tolerance) ||
                wps.ThicknessMax > (pqr.ThicknessQualifiedMax + tolerance))
            {
                return (false,
                    $"Thickness out of range ({wps.ThicknessMin}-{wps.ThicknessMax}) vs ({pqr.ThicknessQualifiedMin}-{pqr.ThicknessQualifiedMax})");
            }

            // =========================
            // 4. F-NUMBER
            // =========================
            if (!string.IsNullOrWhiteSpace(wps.FNumber) &&
                !string.IsNullOrWhiteSpace(pqr.FNumber) &&
                !EqualsIgnoreCase(wps.FNumber, pqr.FNumber))
            {
                return (false, $"F-Number mismatch: {wps.FNumber} vs {pqr.FNumber}");
            }

            // =========================
            // 5. POSITION
            // =========================
            if (!IsPositionQualified(wps.Position ?? "", pqr.Position ?? ""))
                return (false, $"Position not qualified: {wps.Position} vs {pqr.QualifiedPosition}");

            // =========================
            // 6. JOINT TYPE
            // =========================
            if (!string.IsNullOrWhiteSpace(wps.JointType) &&
                !EqualsIgnoreCase(wps.JointType, pqr.JointType))
            {
                return (false, $"Joint mismatch: {wps.JointType} vs {pqr.JointType}");
            }

            // =========================
            // 7. DIAMETER
            // =========================
            if (wps.Diameter > 0 && pqr.DiameterMax > 0)
            {
                if (wps.Diameter < pqr.DiameterMin ||
                    wps.Diameter > pqr.DiameterMax)
                {
                    return (false, $"Diameter out of range: {wps.Diameter}");
                }
            }

            // =========================
            // ✅ PASS
            // =========================
            return (true, "WPS COMPLIANT (ASME IX)");
        }

        // =========================
        // HELPERS
        // =========================
        private bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPositionQualified(string wpsPos, string pqrPos)
        {
            if (string.IsNullOrWhiteSpace(wpsPos) || string.IsNullOrWhiteSpace(pqrPos))
                return true;

            wpsPos = wpsPos.ToUpper();
            pqrPos = pqrPos.ToUpper();

            // ✅ WPS says ALL → always OK
            if (wpsPos == "ALL")
                return true;

            // 6G = all positions
            if (pqrPos == "6G")
                return true;

            // exact match
            if (wpsPos == pqrPos)
                return true;

            // 5G qualification logic
            if (pqrPos == "5G" && (wpsPos == "1G" || wpsPos == "2G"))
                return true;

            return false;
        }
    }
}
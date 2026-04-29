using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class WpsValidationService
    {
        private readonly EssentialVariableEngine _engine = new();

        public List<string> Validate(Wps wps, Pqr pqr)
        {
            var errors = new List<string>();

            if (wps == null)
            {
                errors.Add("No WPS provided");
                return errors;
            }

            if (pqr == null)
            {
                errors.Add("No PQR linked");
                return errors;
            }

            // =========================
            // 🔥 ESSENTIAL VARIABLE ENGINE
            // =========================
            var results = _engine.Evaluate(wps, pqr);

            foreach (var r in results.Where(r => r.IsFailure))
            {
                errors.Add($"{r.Code}: {r.Message}");
            }

            // =========================
            // KEEP YOUR EXISTING RULES (GOOD ONES)
            // =========================

            // PROCESS
            if (!EqualsIgnoreCase(wps.Process, pqr.Process))
                errors.Add($"Process mismatch: {wps.Process} vs {pqr.Process}");

            // THICKNESS
            if (wps.ThicknessMin < pqr.ThicknessQualifiedMin ||
                wps.ThicknessMax > pqr.ThicknessQualifiedMax)
            {
                errors.Add($"Thickness out of range ({wps.ThicknessMin}-{wps.ThicknessMax}) vs ({pqr.ThicknessQualifiedMin}-{pqr.ThicknessQualifiedMax})");
            }

            // DIAMETER
            if (wps.Diameter > 0 && pqr.DiameterMax > 0)
            {
                if (wps.Diameter < pqr.DiameterMin ||
                    wps.Diameter > pqr.DiameterMax)
                {
                    errors.Add($"Diameter out of range: {wps.Diameter}");
                }
            }

            return errors;
        }

        private bool EqualsIgnoreCase(string? a, string? b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
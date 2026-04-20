using WeldAdminPro.Core.Quality;

public class EssentialVariableValidationService
{
    public (bool IsValid, List<string> Errors) Validate(Wps wps, Pqr pqr)
    {
        var errors = new List<string>();

        if (wps == null || pqr == null)
        {
            errors.Add("Missing WPS or PQR");
            return (false, errors);
        }

        // 🔹 Process
        if (!string.Equals(wps.Process, pqr.Process, StringComparison.OrdinalIgnoreCase))
            errors.Add("Process mismatch");

        // 🔹 Material Group
        if (!IsMaterialGroupQualified(wps.MaterialGroup, pqr.MaterialGroup))
            errors.Add("Material group not qualified by PQR");

        // 🔹 Thickness
        if (wps.ThicknessMin > pqr.ThicknessTested || wps.ThicknessMax < pqr.ThicknessTested)
            errors.Add("Thickness not qualified by PQR");

        // 🔹 Position
        if (!string.IsNullOrEmpty(wps.Position) &&
            !string.Equals(wps.Position, pqr.Position, StringComparison.OrdinalIgnoreCase))
            errors.Add("Position not qualified");

        // 🔹 Filler Material
        if (!string.IsNullOrEmpty(wps.FillerMaterial) &&
            !string.Equals(wps.FillerMaterial, pqr.FillerMaterial, StringComparison.OrdinalIgnoreCase))
            errors.Add("Filler material mismatch");

        // 🔹 Gas
        if (!string.IsNullOrEmpty(wps.GasType) &&
            !string.Equals(wps.GasType, pqr.GasType, StringComparison.OrdinalIgnoreCase))
            errors.Add("Shielding gas mismatch");

        // 🔹 Amps
        if (wps.AmpsMin > 0 && pqr.AmpsUsed < wps.AmpsMin)
            errors.Add("Amps below WPS minimum");

        if (wps.AmpsMax > 0 && pqr.AmpsUsed > wps.AmpsMax)
            errors.Add("Amps above WPS maximum");

        // 🔹 Volts
        if (wps.VoltsMin > 0 && pqr.VoltsUsed < wps.VoltsMin)
            errors.Add("Volts below WPS minimum");

        if (wps.VoltsMax > 0 && pqr.VoltsUsed > wps.VoltsMax)
            errors.Add("Volts above WPS maximum");

        // 🔹 Heat Input
        if (wps.HeatInputMin > 0 && pqr.HeatInput < wps.HeatInputMin)
            errors.Add("Heat input too low");

        if (wps.HeatInputMax > 0 && pqr.HeatInput > wps.HeatInputMax)
            errors.Add("Heat input too high");

        // 🔹 Preheat
        if (wps.PreheatMin > 0 && pqr.Preheat < wps.PreheatMin)
            errors.Add("Preheat below minimum");

        if (wps.PreheatMax > 0 && pqr.Preheat > wps.PreheatMax)
            errors.Add("Preheat above maximum");

        // 🔹 Interpass
        if (wps.InterpassMax > 0 && pqr.Interpass > wps.InterpassMax)
            errors.Add("Interpass temperature too high");

        // 🔹 PWHT
        if (wps.PwhtRequired && !pqr.PwhtPerformed)
            errors.Add("PWHT required but not performed");

        return (!errors.Any(), errors);
    }

    // ✅ HELPER METHOD (THIS WAS MISSING)
    private bool IsMaterialGroupQualified(string wpsGroup, string pqrGroup)
    {
        if (string.IsNullOrEmpty(wpsGroup) || string.IsNullOrEmpty(pqrGroup))
            return false;

        // Simple logic (expand later)
        return wpsGroup.StartsWith(pqrGroup);
    }
}
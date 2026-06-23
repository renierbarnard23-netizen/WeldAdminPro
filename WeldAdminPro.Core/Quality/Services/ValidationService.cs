using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality.Services
{
    public class ValidationService
    {
        public ValidationResult Validate(Wps wps, Pqr pqr)
        {
            var result = new ValidationResult();

            // 1️⃣ PQR LINK
            if (pqr == null)
            {
                result.Errors.Add("No PQR linked to WPS");
                return result;
            }

            // 2️⃣ PROCESS
            if (!string.Equals(wps.Process, pqr.Process, StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add($"Process mismatch (WPS: {wps.Process}, PQR: {pqr.Process})");
            }

            // 3️⃣ P-NUMBER
            if (!string.Equals(wps.PNumber, pqr.PNumber))
            {
                result.Errors.Add($"P-No mismatch (WPS: {wps.PNumber}, PQR: {pqr.PNumber})");
            }

            // 4️⃣ F-NUMBER
            if (!string.Equals(wps.FNumber, pqr.FNumber))
            {
                result.Errors.Add($"F-No mismatch (WPS: {wps.FNumber}, PQR: {pqr.FNumber})");
            }

            // 5️⃣ THICKNESS
            if (wps.ThicknessMax > pqr.ThicknessQualifiedMax)
            {
                result.Errors.Add(
                    $"Thickness exceeds PQR (WPS: {wps.ThicknessMax} mm > PQR: {pqr.ThicknessQualifiedMax} mm)");
            }

            if (wps.ThicknessMin < pqr.ThicknessQualifiedMin)
            {
                result.Errors.Add(
                    $"Minimum thickness below qualified range (WPS: {wps.ThicknessMin} mm < PQR: {pqr.ThicknessQualifiedMin} mm)");
            }

            return result;
        }
    }
}
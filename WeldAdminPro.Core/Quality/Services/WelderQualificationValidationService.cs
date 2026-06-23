using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WelderQualificationValidationService
    {
        public bool IsWelderQualified(
            WelderQualification qualification,
            string requiredProcess,
            string requiredPosition,
            string requiredMaterialGroup,
            double requiredThickness)
        {
            // Qualification expired
            if (qualification.ExpiryDate < DateTime.Today)
                return false;

            // Process mismatch
            if (!string.Equals(
                    qualification.Process,
                    requiredProcess,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            // Position mismatch
            if (!string.Equals(
                    qualification.Position,
                    requiredPosition,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            // Material group mismatch
            if (!string.Equals(
                    qualification.MaterialGroup,
                    requiredMaterialGroup,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            // Thickness outside qualified range
            if (requiredThickness < qualification.ThicknessMin ||
                requiredThickness > qualification.ThicknessMax)
                return false;

            return true;
        }

        public bool IsWelderQualifiedForWps(
    WelderQualification qualification,
    Wps wps,
    double weldThickness)
        {
            if (qualification.ExpiryDate < DateTime.Today)
                return false;

            if (!string.Equals(
                    qualification.Process,
                    wps.Process,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(
                    qualification.Position,
                    wps.Position,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(
                    qualification.MaterialGroup,
                    wps.MaterialGroup,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (weldThickness < qualification.ThicknessMin ||
                weldThickness > qualification.ThicknessMax)
                return false;

            if (weldThickness < wps.ThicknessMin ||
                weldThickness > wps.ThicknessMax)
                return false;

            return true;
        }
    }
}
using WeldAdminPro.Core.Quality.Normalization;
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
            // Qualification inactive
            if (!qualification.IsActive)
                return false;

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
            // Qualification inactive
            if (!qualification.IsActive)
                return false;

            if (qualification.ExpiryDate < DateTime.Today)
                return false;

            var qualificationProcess =
                ComplianceNormalizer.NormalizeProcess(
                    qualification.Process);

            var wpsProcess =
                ComplianceNormalizer.NormalizeProcess(
                    wps.Process);

            if (qualificationProcess != wpsProcess)
                return false;

            var qualificationPosition =
                ComplianceNormalizer.NormalizePosition(
                    qualification.Position);

            var wpsPosition =
                ComplianceNormalizer.NormalizePosition(
                    wps.Position ?? "");

            if (wpsPosition != "ALL" &&
                qualificationPosition != wpsPosition)
                return false;

            var qualificationMaterial =
                ComplianceNormalizer.NormalizeMaterialGroup(
                    qualification.MaterialGroup);

            var wpsMaterial =
                ComplianceNormalizer.NormalizeMaterialGroup(
                    wps.MaterialGroup);

            if (qualificationMaterial != wpsMaterial)
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




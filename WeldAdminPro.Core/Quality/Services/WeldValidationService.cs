using System;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Normalization;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldValidationService
    {
        public WeldValidationResult Validate(
            WelderQualification qualification,
            Wps wps,
            double weldThickness)
        {
            var result = new WeldValidationResult();

            // =====================================
            // EXPIRY VALIDATION
            // =====================================

            if (qualification.ExpiryDate < DateTime.Today)
            {
                result.Errors.Add(
                    "Welder qualification expired.");
            }

            // =====================================
            // PROCESS VALIDATION
            // =====================================

            var qualificationProcess =
                ComplianceNormalizer.NormalizeProcess(
                    qualification.Process);

            var wpsProcess =
                ComplianceNormalizer.NormalizeProcess(
                    wps.Process);

            if (qualificationProcess != wpsProcess)
            {
                result.Errors.Add(
                    $"Process mismatch: " +
                    $"{qualificationProcess} vs {wpsProcess}");
            }

            // =====================================
            // POSITION VALIDATION
            // =====================================

            var qualificationPosition =
                ComplianceNormalizer.NormalizePosition(
                    qualification.Position);

            var wpsPosition =
                ComplianceNormalizer.NormalizePosition(
                    wps.Position ?? "");

            bool positionValid =
                wpsPosition == "ALL"
                || qualificationPosition == wpsPosition;

            if (!positionValid)
            {
                result.Errors.Add(
                    "Welder not qualified for position.");
            }

            // =====================================
            // MATERIAL GROUP VALIDATION
            // =====================================

            var qualificationMaterial =
                ComplianceNormalizer.NormalizeMaterialGroup(
                    qualification.MaterialGroup);

            var wpsMaterial =
                ComplianceNormalizer.NormalizeMaterialGroup(
                    wps.MaterialGroup);

            bool materialValid =
                qualificationMaterial == wpsMaterial;
            
            if (!materialValid)
            {
                result.Errors.Add(
                    "Welder not qualified for material group.");
            }

            // =====================================
            // WELDER THICKNESS VALIDATION
            // =====================================

            if (weldThickness < qualification.ThicknessMin
                || weldThickness > qualification.ThicknessMax)
            {
                result.Errors.Add(
                    "Thickness outside welder qualification range.");
            }

            // =====================================
            // WPS THICKNESS VALIDATION
            // =====================================

            if (weldThickness < wps.ThicknessMin
                || weldThickness > wps.ThicknessMax)
            {
                result.Errors.Add(
                    "Thickness outside WPS qualified range.");
            }

            // =====================================
            // FINAL RESULT
            // =====================================

            result.IsValid = !result.Errors.Any();

            return result;
        }
    }
}

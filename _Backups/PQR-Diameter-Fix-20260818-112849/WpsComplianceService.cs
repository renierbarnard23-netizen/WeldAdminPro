using System.Linq;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.Core.Services
{
    public class WpsComplianceService
    {
        private readonly WpsValidationService _validation = new();
        private readonly EssentialVariableEngine _essential = new();
        private readonly QualificationRangeEngine _range = new();
        private readonly PositionQualificationService _positionService = new();
        private readonly ThicknessQualificationService _thicknessService = new();
        private readonly DiameterQualificationService _diameterService = new();
        private readonly MaterialQualificationService _materialService = new();

        public WpsComplianceResult Evaluate(Wps wps, Pqr pqr)
        {
            var result = new WpsComplianceResult();

            if (!result.IsCompliant)
            {
                result.ValidationErrors.ForEach(
                    x => System.Diagnostics.Debug.WriteLine("Validation: " + x));

                result.EssentialFailures.ForEach(
                    x => System.Diagnostics.Debug.WriteLine("Essential: " + x));

                result.RangeFailures.ForEach(
                    x => System.Diagnostics.Debug.WriteLine("Range: " + x));
            }

            if (wps == null)
            {
                result.ValidationErrors.Add("WPS is null");
                return result;
            }

            if (pqr == null)
            {
                result.ValidationErrors.Add("No PQR linked");
                return result;
            }

            System.Diagnostics.Debug.WriteLine("");

            #if DEBUG
            System.Diagnostics.Debug.WriteLine("===== WPS =====");
            System.Diagnostics.Debug.WriteLine($"WPS No: {wps.WpsNumber}");
            #endif

            System.Diagnostics.Debug.WriteLine($"Process: {wps.Process}");
            System.Diagnostics.Debug.WriteLine($"PNumber: {wps.PNumber}");
            System.Diagnostics.Debug.WriteLine($"FNumber: {wps.FNumber}");
            System.Diagnostics.Debug.WriteLine($"Position: '{wps.Position}'");
            System.Diagnostics.Debug.WriteLine($"JointType: {wps.JointType}");
            System.Diagnostics.Debug.WriteLine($"JointDesign: '{wps.JointDesign}'");
            System.Diagnostics.Debug.WriteLine($"CurrentType: '{wps.CurrentType}'");
            System.Diagnostics.Debug.WriteLine($"Progression: '{wps.Progression}'");
            System.Diagnostics.Debug.WriteLine($"Thickness: {wps.ThicknessMin}-{wps.ThicknessMax}");
            System.Diagnostics.Debug.WriteLine($"Diameter: {wps.Diameter}");

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("===== PQR =====");
            System.Diagnostics.Debug.WriteLine($"PQR No: {pqr.PqrNumber}");
            System.Diagnostics.Debug.WriteLine($"Process: {pqr.Process}");
            System.Diagnostics.Debug.WriteLine($"PNumber: {pqr.PNumber}");
            System.Diagnostics.Debug.WriteLine($"FNumber: {pqr.FNumber}");
            System.Diagnostics.Debug.WriteLine($"Position: '{pqr.QualifiedPosition}'");
            System.Diagnostics.Debug.WriteLine($"JointType: {pqr.JointType}");
            System.Diagnostics.Debug.WriteLine($"JointDesign: '{pqr.JointDesign}'");
            System.Diagnostics.Debug.WriteLine($"CurrentType: '{pqr.CurrentType}'");
            System.Diagnostics.Debug.WriteLine($"Progression: '{pqr.Progression}'");
            System.Diagnostics.Debug.WriteLine($"Thickness: {pqr.ThicknessQualifiedMin}-{pqr.ThicknessQualifiedMax}");
            System.Diagnostics.Debug.WriteLine($"Diameter: {pqr.DiameterMin}-{pqr.DiameterMax}");

            // =========================
            // BASIC VALIDATION
            // =========================
            var validationErrors = _validation.Validate(wps, pqr);
            result.ValidationErrors = validationErrors;

            // =========================
            // ESSENTIAL VARIABLES
            // =========================

            var essentialResults = _essential.Evaluate(wps, pqr);

            result.EssentialFailures = essentialResults
                .Where(x => x.IsFailure)
                .Select(x => x.Message)
                .ToList();

            // =====================================
            // MATERIAL VALIDATION
            // =====================================


            
            var materialQualified =
                _materialService.IsQualified(
                    wps.PNumber  ?? "",
                    pqr.PNumber ?? "");

            if (!materialQualified)
            {
                result.RangeFailures.Add(
                    $"QW-423: Material {wps.PNumber} " +
                    $"not qualified by PQR {pqr.PNumber}");
            }

            // =========================
            // QUALIFICATION RANGE
            // =========================

            var range = _range.Calculate(pqr);

            // =====================================
            // THICKNESS VALIDATION
            // =====================================

            var thicknessQualified =
                _thicknessService.IsQualified(
                    wps.ThicknessMin,
                    wps.ThicknessMax,
                    range.MinThickness,
                    range.MaxThickness);

            if (!thicknessQualified)
            {
                result.RangeFailures.Add(
                    $"QW-451: Thickness range " +
                    $"{wps.ThicknessMin}-{wps.ThicknessMax} mm " +
                    $"not qualified by PQR range " +
                    $"{range.MinThickness}-{range.MaxThickness} mm");
            }

            // =====================================
            // DIAMETER VALIDATION
            // =====================================

            System.Diagnostics.Debug.WriteLine(
    $"WPS Diameter: {wps.Diameter}");

            System.Diagnostics.Debug.WriteLine(
                $"Qualified Diameter Range: " +
                $"{range.MinDiameter}-{range.MaxDiameter}");

            var diameterQualified =
                _diameterService.IsQualified(
                    wps.Diameter,
                    range.MinDiameter,
                    range.MaxDiameter);

            if (!diameterQualified)
            {
                result.RangeFailures.Add(
                    $"QW-452: Diameter {wps.Diameter} mm " +
                    $"outside qualified range " +
                    $"{range.MinDiameter}-{range.MaxDiameter} mm");
            }

            // =========================
            // FINAL DECISION
            // =========================

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Validation Errors:");

            foreach (var e in result.ValidationErrors)
            {
                System.Diagnostics.Debug.WriteLine($"'{e}'");
            }

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Essential Failures:");

            foreach (var e in result.EssentialFailures)
            {
                System.Diagnostics.Debug.WriteLine($"'{e}'");
            }

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Range Failures:");

            foreach (var e in result.RangeFailures)
            {
                System.Diagnostics.Debug.WriteLine($"'{e}'");
            }

            result.IsCompliant =
    !result.ValidationErrors.Any() &&
    !result.EssentialFailures.Any() &&
    !result.RangeFailures.Any();

            if (!result.IsCompliant)
            {
                System.Diagnostics.Debug.WriteLine("=== WPS COMPLIANCE FAILED ===");

                foreach (var x in result.ValidationErrors)
                    System.Diagnostics.Debug.WriteLine("Validation: " + x);

                foreach (var x in result.EssentialFailures)
                    System.Diagnostics.Debug.WriteLine("Essential: " + x);

                foreach (var x in result.RangeFailures)
                    System.Diagnostics.Debug.WriteLine("Range: " + x);
            }

            return result;
        }
    }
}
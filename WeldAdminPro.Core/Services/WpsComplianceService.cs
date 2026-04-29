using System.Linq;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class WpsComplianceService
    {
        private readonly WpsValidationService _validation = new();
        private readonly EssentialVariableEngine _essential = new();
        private readonly QualificationRangeEngine _range = new();

        public WpsComplianceResult Evaluate(Wps wps, Pqr pqr)
        {
            var result = new WpsComplianceResult();

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

            // =========================
            // BASIC VALIDATION
            // =========================
            var validationErrors = _validation.Validate(wps, pqr);
            result.ValidationErrors = validationErrors;

            // =========================
            // ESSENTIAL VARIABLES (ASME IX)
            // =========================
            var essentialResults = _essential.Evaluate(wps, pqr);

            result.EssentialFailures = essentialResults
                .Where(x => x.IsFailure)
                .Select(x => x.Message)
                .ToList();

            // =========================
            // QUALIFICATION RANGE
            // =========================
            var rangeResults = _range.Evaluate(wps, pqr);

            result.RangeFailures = rangeResults
                .Where(x => x.IsFailure)
                .Select(x => x.Message)
                .ToList();

            // =========================
            // FINAL DECISION
            // =========================
            result.IsCompliant =
                !result.ValidationErrors.Any() &&
                !result.EssentialFailures.Any() &&
                !result.RangeFailures.Any();

            return result;
        }
    }
}
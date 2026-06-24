using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldReadinessEngine
        : IWeldReadinessEngine
    {
        public WeldReadinessResult Evaluate(
            Weld weld)
        {
            var result =
                new WeldReadinessResult
                {
                    WeldId = weld.Id
                };

            result.HasWps =
                !string.IsNullOrWhiteSpace(
                    weld.WpsNumber);

            result.HasQualifiedWelder =
                !string.IsNullOrWhiteSpace(
                    weld.WelderNumber);

            result.MaterialsAvailable =
                !string.IsNullOrWhiteSpace(
                    weld.MaterialHeat1);

            result.NdtRequirementsDefined =
                !string.IsNullOrWhiteSpace(
                    weld.RequiredNdt);

            result.RequiredDocumentsPresent =
                !string.IsNullOrWhiteSpace(
                    weld.DrawingNumber);

            result.HoldPointsCleared =
                weld.BlockingCount == 0;

            int score = 0;

            if (result.HasWps)
                score += 20;

            if (result.HasQualifiedWelder)
                score += 20;

            if (result.MaterialsAvailable)
                score += 20;

            if (result.NdtRequirementsDefined)
                score += 20;

            if (result.RequiredDocumentsPresent)
                score += 10;

            if (result.HoldPointsCleared)
                score += 10;

            result.ReadinessScore =
                score;

            result.IsReady =
                score >= 90;

            if (!result.HasWps)
                result.BlockingReasons
                    .Add("No WPS assigned.");

            if (!result.HasQualifiedWelder)
                result.BlockingReasons
                    .Add("No qualified welder assigned.");

            if (!result.MaterialsAvailable)
                result.BlockingReasons
                    .Add("Material heat numbers missing.");

            if (!result.NdtRequirementsDefined)
                result.BlockingReasons
                    .Add("NDT requirements not defined.");

            if (!result.RequiredDocumentsPresent)
                result.BlockingReasons
                    .Add("Drawing number missing.");

            if (!result.HoldPointsCleared)
                result.BlockingReasons
                    .Add("Open hold points exist.");

            return result;
        }
    }
}
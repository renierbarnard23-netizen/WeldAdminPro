using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Normalization;

namespace WeldAdminPro.Core.Services
{
    public class MaterialQualificationService
    {
        private readonly Dictionary<string, List<string>>
            _qualificationMatrix =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    { "P1", new List<string> { "P1" } },
                    { "P8", new List<string> { "P8" } },
                    { "P41", new List<string> { "P41" } },
                    { "P10H", new List<string> { "P10H" } }
                };

        public bool IsQualified(
            string requiredMaterial,
            string qualifiedMaterial)
        {
            requiredMaterial =
                ComplianceNormalizer
                    .NormalizeMaterialGroup(
                        requiredMaterial);

            qualifiedMaterial =
                ComplianceNormalizer
                    .NormalizeMaterialGroup(
                        qualifiedMaterial);

            System.Diagnostics.Debug.WriteLine(
                $"Required Material = [{requiredMaterial}]");

            System.Diagnostics.Debug.WriteLine(
                $"Qualified Material = [{qualifiedMaterial}]");

            if (string.IsNullOrWhiteSpace(requiredMaterial))
                return true;

            if (string.IsNullOrWhiteSpace(qualifiedMaterial))
                return true;

            // Exact match
            if (requiredMaterial ==
                qualifiedMaterial)
            {
                return true;
            }

            // Matrix lookup
            if (_qualificationMatrix.TryGetValue(
                    qualifiedMaterial,
                    out var qualifiedMaterials))
            {
                return qualifiedMaterials.Any(
                    x => x.Equals(
                        requiredMaterial,
                        StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}
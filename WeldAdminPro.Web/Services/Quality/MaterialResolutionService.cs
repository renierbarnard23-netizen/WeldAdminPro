using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Web.Services.Quality;

public class MaterialResolutionService
{
    private readonly MaterialLibraryService _materialLibrary;

    public MaterialResolutionService(
        MaterialLibraryService materialLibrary)
    {
        _materialLibrary = materialLibrary;
    }

    public BaseMaterial? Resolve(
        string? material,
        string? specification,
        string? grade,
        string? uns,
        string? pNumber)
    {
        // ========================================================
        // AUTHORITATIVE MATERIAL RESOLUTION
        //
        // The PQR material information is the source.
        //
        // Priority:
        //   1. UNS
        //   2. Specification + Grade
        //   3. Material + Specification + Grade
        //   4. Material + Specification
        //   5. Material
        //
        // P-Number is deliberately NOT used to select a material.
        // ========================================================

        var library = _materialLibrary.BaseMaterials;

        // --------------------------------------------------------
        // 1. EXACT UNS
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(uns))
        {
            var normalizedUns =
                uns.Trim().ToUpperInvariant();

            var unsMatches =
                library
                    .Where(m =>
                        string.Equals(
                            m.UNS?.Trim(),
                            normalizedUns,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (unsMatches.Count == 1)
                return unsMatches[0];

            // If UNS exists but is ambiguous, do not guess.
            if (unsMatches.Count > 1)
            {
                var narrowed =
                    unsMatches.Where(m =>
                        string.IsNullOrWhiteSpace(grade) ||
                        string.Equals(
                            m.Grade?.Trim(),
                            grade.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (narrowed.Count == 1)
                    return narrowed[0];

                return null;
            }
        }

        // --------------------------------------------------------
        // 2. SPECIFICATION + GRADE
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(specification) &&
            !string.IsNullOrWhiteSpace(grade))
        {
            var matches =
                library.Where(m =>
                    string.Equals(
                        m.Specification?.Trim(),
                        specification.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        m.Grade?.Trim(),
                        grade.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 1)
                return matches[0];

            if (matches.Count > 1 &&
                !string.IsNullOrWhiteSpace(material))
            {
                var narrowed =
                    matches.Where(m =>
                        string.Equals(
                            m.Material?.Trim(),
                            material.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (narrowed.Count == 1)
                    return narrowed[0];
            }
        }

        // --------------------------------------------------------
        // 3. MATERIAL + SPECIFICATION + GRADE
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(material) &&
            !string.IsNullOrWhiteSpace(specification) &&
            !string.IsNullOrWhiteSpace(grade))
        {
            var match =
                library.FirstOrDefault(m =>
                    string.Equals(
                        m.Material?.Trim(),
                        material.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        m.Specification?.Trim(),
                        specification.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        m.Grade?.Trim(),
                        grade.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match;
        }

        // --------------------------------------------------------
        // 4. MATERIAL + SPECIFICATION
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(material) &&
            !string.IsNullOrWhiteSpace(specification))
        {
            var matches =
                library.Where(m =>
                    string.Equals(
                        m.Material?.Trim(),
                        material.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        m.Specification?.Trim(),
                        specification.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 1)
                return matches[0];

            // Do not guess between multiple grades.
            if (matches.Count > 1 &&
                !string.IsNullOrWhiteSpace(grade))
            {
                var narrowed =
                    matches.Where(m =>
                        string.Equals(
                            m.Grade?.Trim(),
                            grade.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (narrowed.Count == 1)
                    return narrowed[0];
            }
        }

        // --------------------------------------------------------
        // 5. MATERIAL ONLY
        //
        // Only accept if there is exactly one library record.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(material))
        {
            var matches =
                library.Where(m =>
                    string.Equals(
                        m.Material?.Trim(),
                        material.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 1)
                return matches[0];
        }

        // --------------------------------------------------------
        // 6. NO CONFIDENT MATCH
        //
        // Never guess using P-Number.
        // --------------------------------------------------------

        return null;
    }
    public void ApplyToPqr(
        WeldAdminPro.Core.Quality.Pqr pqr,
        BaseMaterial material)
    {
        pqr.BaseMaterial1 =
            material.Material ?? "";

        pqr.BaseMaterial1Specification =
            material.Specification ?? "";

        pqr.PNumber =
            material.PNumber.ToString();

        pqr.MaterialGroup =
            material.GroupNumber.ToString();
    }
}



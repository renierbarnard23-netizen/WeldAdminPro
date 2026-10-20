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
        string? pNumber)
    {
        BaseMaterial? result = null;

        // ----------------------------------------------------
        // 1. Exact Material + Specification
        // ----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(material))
        {
            result =
                _materialLibrary.BaseMaterials.FirstOrDefault(m =>
                    string.Equals(
                        m.Material ?? "",
                        material.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        m.Specification ?? "",
                        specification?.Trim() ?? "",
                        StringComparison.OrdinalIgnoreCase));
        }

        // ----------------------------------------------------
        // 2. Material only
        // ----------------------------------------------------

        if (result == null &&
            !string.IsNullOrWhiteSpace(material))
        {
            result =
                _materialLibrary.BaseMaterials.FirstOrDefault(m =>
                    string.Equals(
                        m.Material ?? "",
                        material.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        // ----------------------------------------------------
        // 3. P-Number
        //
        // Supports:
        // P51
        // 51
        // P-51
        // ----------------------------------------------------

        if (result == null &&
            !string.IsNullOrWhiteSpace(pNumber))
        {
            var normalized =
                pNumber
                    .Trim()
                    .ToUpperInvariant()
                    .Replace("P-", "")
                    .Replace("P", "")
                    .Trim();

            if (int.TryParse(normalized, out var parsedPNumber))
            {
                result =
                    _materialLibrary.BaseMaterials.FirstOrDefault(m =>
                        m.PNumber == parsedPNumber);
            }
        }

        return result;
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


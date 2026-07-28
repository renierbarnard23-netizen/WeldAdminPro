using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Web.Services.Quality;

public class FillerMaterialSearchService
{
    private readonly MaterialLibraryService _library;

    public FillerMaterialSearchService(MaterialLibraryService library)
    {
        _library = library;
    }

    public Task<IEnumerable<FillerMaterial>> Search(string? value)
    {
        IEnumerable<FillerMaterial> result = _library.Fillers;

        if (!string.IsNullOrWhiteSpace(value))
        {
            result = result.Where(f =>

                f.Classification.Contains(value, StringComparison.OrdinalIgnoreCase)

                || f.AwsClassification.Contains(value, StringComparison.OrdinalIgnoreCase)

                || f.SfaNumber.Contains(value, StringComparison.OrdinalIgnoreCase)

                || f.FillerComposition.Contains(value, StringComparison.OrdinalIgnoreCase)

                || f.FillerForm.Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        result = result
            .OrderBy(f => f.FillerComposition)
            .ThenBy(f => f.Classification);

        return Task.FromResult(result);
    }
}
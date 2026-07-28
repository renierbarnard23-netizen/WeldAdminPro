using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Web.Services.Quality;

public class MaterialSearchService
{
    private readonly MaterialLibraryService _library;

    public MaterialSearchService(MaterialLibraryService library)
    {
        _library = library;
    }

    public IEnumerable<BaseMaterial> Search(string? text)
    {
        var query = _library.BaseMaterials.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(text))
        {
            text = text.Trim();

            query = query.Where(m =>
                (m.Material ?? "").Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (m.Specification ?? "").Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (m.Grade ?? "").Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (m.UNS ?? "").Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (m.Category ?? "").Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (m.Description ?? "").Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Specification)
            .ThenBy(m => m.Grade);
    }
}
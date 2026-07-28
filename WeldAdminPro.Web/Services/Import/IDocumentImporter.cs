using WeldAdminPro.Web.Models.Import;

namespace WeldAdminPro.Web.Services.Import;

public interface IDocumentImporter
{
    Task<DocumentImportResult> ImportAsync(
        Stream stream,
        string fileName);
}
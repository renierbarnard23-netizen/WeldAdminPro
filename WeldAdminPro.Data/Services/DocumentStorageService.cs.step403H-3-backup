
namespace WeldAdminPro.Data.Services;

public class DocumentStorageService
{
    private readonly string _rootFolder;

    public DocumentStorageService()
    {
        _rootFolder = Path.Combine(
            AppContext.BaseDirectory,
            "Documents");
    }

    public async Task<string> SaveProjectDocumentAsync(
        Guid projectId,
        string fileName,
        Stream stream)
    {
        var folder = Path.Combine(
            _rootFolder,
            "Projects",
            projectId.ToString());

        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);

        await using var fileStream =
            new FileStream(fullPath, FileMode.Create);

        await stream.CopyToAsync(fileStream);

        return fullPath;
    }
}
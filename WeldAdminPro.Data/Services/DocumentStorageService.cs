
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

        var fullPath = GetAvailableFilePath(folder, fileName);

        await using var fileStream =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
        await stream.CopyToAsync(fileStream);

        return fullPath;
    }

    public async Task<string> SaveCompanyDocumentAsync(
        Guid companyId,
        string fileName,
        Stream stream)
    {
        var folder = Path.Combine(
            _rootFolder,
            "Companies",
            companyId.ToString());

        Directory.CreateDirectory(folder);

        var fullPath = GetAvailableFilePath(folder, fileName);

        await using var fileStream =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

        await stream.CopyToAsync(fileStream);

        return fullPath;
    }
    public async Task<string> SaveWeldDocumentAsync(
        Guid weldId,
        string fileName,
        Stream stream)
    {
        if (weldId == Guid.Empty)
            throw new ArgumentException(
                "Weld ID is required.",
                nameof(weldId));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var safeFileName =
            Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException(
                "A valid file name is required.",
                nameof(fileName));

        var folder = Path.Combine(
            _rootFolder,
            "Welds",
            weldId.ToString());

        Directory.CreateDirectory(folder);

        var fullPath =
            GetAvailableFilePath(folder, safeFileName);

        await using var fileStream =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

        await stream.CopyToAsync(fileStream);

        return fullPath;
    }
    private static string GetAvailableFilePath(
        string folder,
        string fileName)
    {
        var safeFileName = Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException(
                "A valid file name is required.",
                nameof(fileName));

        var extension = Path.GetExtension(safeFileName);
        var nameWithoutExtension =
            Path.GetFileNameWithoutExtension(safeFileName);

        var candidate = Path.Combine(folder, safeFileName);
        var counter = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(
                folder,
                $"{nameWithoutExtension} ({counter}){extension}");

            counter++;
        }

        return candidate;
    }
}


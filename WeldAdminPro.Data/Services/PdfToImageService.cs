using System.Diagnostics;

namespace WeldAdminPro.Data.Services;

public class PdfToImageService
{
    public List<string> ConvertToImages(string pdfPath)
    {
        var outputPrefix = Path.Combine(
            Path.GetTempPath(),
            $"import_{Guid.NewGuid()}");

        var process = new Process();

        process.StartInfo.FileName = "pdftoppm";

        process.StartInfo.Arguments =
            $"-png \"{pdfPath}\" \"{outputPrefix}\"";

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        process.WaitForExit();

        var images = Directory
            .GetFiles(Path.GetTempPath(), Path.GetFileName(outputPrefix) + "*.png")
            .OrderBy(f => f)
            .ToList();

        if (images.Count == 0)
            throw new Exception("No images were generated.");

        return images;
    }
    public string ConvertFirstPage(string pdfPath)
    {
        return ConvertToImages(pdfPath).First();
    }
}
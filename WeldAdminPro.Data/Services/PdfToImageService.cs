using System.Diagnostics;
using System.IO;

namespace WeldAdminPro.Data.Services
{
    public class PdfToImageService
    {
        public string ConvertFirstPage(string pdfPath)
        {
            var outputImage = Path.Combine(
                Path.GetTempPath(),
                $"wps_{System.Guid.NewGuid()}.png");

            var process = new Process();
            process.StartInfo.FileName = "pdftoppm";
            process.StartInfo.Arguments = $"-png \"{pdfPath}\" \"{outputImage}\" -singlefile";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();
            process.WaitForExit();

            return outputImage + ".png";
        }
    }
}
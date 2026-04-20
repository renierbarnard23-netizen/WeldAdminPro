using System.Drawing;
using System.Drawing.Imaging;
using ImgFormat = System.Drawing.Imaging.ImageFormat;
using System.IO;
using Tesseract;
using System.Runtime.Versioning;

namespace WeldAdminPro.Data.Services
{
    public class TesseractService
    {
        [SupportedOSPlatform("windows")]
        public string ExtractText(Bitmap image)
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

            using var ms = new MemoryStream();

            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("OCR only supported on Windows");

            image.Save(ms, ImgFormat.Png);
            ms.Position = 0;

            using var pix = Pix.LoadFromMemory(ms.ToArray());
            using var page = engine.Process(pix);

            return page.GetText();
        }
    }
}
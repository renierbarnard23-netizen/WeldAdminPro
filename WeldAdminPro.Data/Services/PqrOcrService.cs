using System.Drawing;
using System.Runtime.Versioning;

namespace WeldAdminPro.Data.Services
{
    public class PqrOcrService
    {
        private readonly TesseractService _tesseract = new();

        [SupportedOSPlatform("windows")]
        public string ExtractTextFromImage(Bitmap image)
        {
            return _tesseract.ExtractText(image);
        }
    }
}
using System.Drawing;
using System.Runtime.Versioning;

namespace WeldAdminPro.Data.Services
{
    public class PqrOcrService
    {
        private readonly TesseractService _tesseract;

        public PqrOcrService(TesseractService tesseract)
        {
            _tesseract = tesseract;
        }

        [SupportedOSPlatform("windows")]
        public string ExtractTextFromImage(Bitmap image)
        {
            return _tesseract.ExtractText(image);
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using Tesseract;

namespace WeldAdminPro.Data.Services
{
    public class WpsOcrService
    {
        public string ExtractText(string imagePath)
        {
            try
            {
                var tessPath = Path.Combine(
     AppDomain.CurrentDomain.BaseDirectory,
     "tessdata");

                if (!Directory.Exists(tessPath))
                {
                    throw new Exception($"Tessdata folder not found: {tessPath}");
                }

                using var engine = new TesseractEngine(tessPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);

                var text = page.GetText();

                Debug.WriteLine("🧠 OCR TEXT:");
                Debug.WriteLine(text);

                return text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OCR ERROR: {ex.Message}");
                return "";
            }
        }
    }
}
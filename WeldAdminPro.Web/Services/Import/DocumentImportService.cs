using System.Drawing;
using System.Text;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Web.Models.Import;
using WeldAdminPro.Data.Models.OCR;
using WeldAdminPro.Data.Services.Recognition;
using System.Runtime.Versioning;

namespace WeldAdminPro.Web.Services.Import;

[SupportedOSPlatform("windows")]
public class DocumentImportService : IDocumentImporter
{
    private readonly PdfToImageService _pdfService;
    private readonly PqrOcrService _ocrService;
    private readonly PqrParserService _parser;
    private readonly RecognitionEngine _recognitionEngine;

    public DocumentImportService(
        PdfToImageService pdfService,
        PqrOcrService ocrService,
        PqrParserService parser,
        RecognitionEngine recognitionEngine)
    {
        _pdfService = pdfService;
        _ocrService = ocrService;
        _parser = parser;
        _recognitionEngine = recognitionEngine;
    }

    public async Task<DocumentImportResult> ImportAsync(
        Stream stream,
        string fileName)
    {
        var tempPdf = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.pdf");

        try
        {
            // Save uploaded PDF
            using (var fs = File.Create(tempPdf))
            {
                await stream.CopyToAsync(fs);
            }

            // Convert PDF pages to PNG images
            var images = _pdfService.ConvertToImages(tempPdf);

            var document = new OcrDocument();

            int pageNumber = 1;

            foreach (var imagePath in images)
            {
                using var bitmap = new Bitmap(imagePath);

                var text = _ocrService.ExtractTextFromImage(bitmap);

                document.Pages.Add(new OcrPage
                {
                    PageNumber = pageNumber++,
                    ImagePath = imagePath,
                    Text = text
                });
            }

            Console.WriteLine("========== OCR DOCUMENT ==========");

            foreach (var page in document.Pages)
            {
                Console.WriteLine($"Page {page.PageNumber}");
                Console.WriteLine(
                    $"Page {page.PageNumber} : {page.Text.Length} characters");
            }

            Console.WriteLine("===============================");

            // Keep existing parser working
            var rawText = document.FullText;

            var pqr = _parser.Parse(document, fileName);


            // --------------------------------------------------------
            // Preserve the structured material recognition result.
            // RecognitionEngine is authoritative for:
            // Specification, Grade, UNS and P-Number.
            // --------------------------------------------------------
            var recognitionContext = new OcrRecognitionContext(document);

            var recognition =
                _recognitionEngine.Recognize(recognitionContext);

            return new DocumentImportResult
            {
                FileName = fileName,
                Status = ImportStatus.Completed,
                RawText = rawText,

                MaterialSpecification = recognition.MaterialSpecification ?? string.Empty,
                MaterialGrade = recognition.MaterialGrade ?? string.Empty,
                MaterialUNS = recognition.MaterialUNS ?? string.Empty,
                MaterialPNumber = recognition.PNumber ?? string.Empty,
                Pqr = pqr
            };
        }
        catch (Exception ex)
        {
            return new DocumentImportResult
            {
                FileName = fileName,
                Status = ImportStatus.Failed,
                RawText = ex.ToString()
            };
        }
        finally
        {
            if (File.Exists(tempPdf))
                File.Delete(tempPdf);
        }
    }
}

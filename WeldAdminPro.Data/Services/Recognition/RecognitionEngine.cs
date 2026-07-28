using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Models.OCR;

namespace WeldAdminPro.Data.Services.Recognition;

public class RecognitionEngine
{
    private readonly MaterialRecognitionService _materialRecognition;
    private readonly SmartMaterialExtractor _materialExtractor;
    private readonly TextNormalizationService _textNormalization;
    private readonly PNumberRecognitionService _pNumberRecognition;
    private readonly SpecificationScanner _specificationScanner;

    public RecognitionEngine(
        MaterialRecognitionService materialRecognition,
        SmartMaterialExtractor materialExtractor,
        TextNormalizationService textNormalization,
        PNumberRecognitionService pNumberRecognition,
        SpecificationScanner specificationScanner)
    {
        _materialRecognition = materialRecognition;
        _materialExtractor = materialExtractor;
        _textNormalization = textNormalization;
        _pNumberRecognition = pNumberRecognition;
        _specificationScanner = specificationScanner;
    }


    public RecognitionResult Recognize(OcrRecognitionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Console.WriteLine("===== OCR RECOGNITION CONTEXT =====");
        Console.WriteLine($"Page 1 Characters : {context.FirstPageText.Length}");
        Console.WriteLine($"Full Document     : {context.FullText.Length}");
        Console.WriteLine($"Remaining Pages   : {context.RemainingPagesText.Length}");
        Console.WriteLine("===================================");

        // Phase 5:
        // Material recognition only uses Page 1.
        return Recognize(context.FirstPageText);
    }
    public RecognitionResult Recognize(OcrDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Console.WriteLine("===== OCR DOCUMENT =====");

        foreach (var page in document.Pages)
        {
            Console.WriteLine($"Page {page.PageNumber} : {page.Text.Length} chars");
        }

        Console.WriteLine("========================");

        var context = new OcrRecognitionContext(document);

        return Recognize(context);
    }

    public RecognitionResult Recognize(string text)
    {
        text ??= "";

        var result = new RecognitionResult();

        // Normalize OCR first
        text = _textNormalization.Normalize(text);

        Console.WriteLine("===== OCR HEADER (FIRST 800 CHARS) =====");
        Console.WriteLine(text[..Math.Min(800, text.Length)]);
        Console.WriteLine("========================================");

        // Then extract sections
        var extractedText = _materialExtractor.Extract(text);

        // Then scan normalized text
        var scannedText = _specificationScanner.Scan(text);

        // Keep both for diagnostics.
        var materialText =
            extractedText +
            Environment.NewLine +
            scannedText;

        result.MaterialText = materialText;

        // ----------------------------------------------------
        // PRIMARY MATERIAL RECOGNITION
        // ----------------------------------------------------

        // First try the extracted BASE METALS section.
        result.Material =
            _materialRecognition.Recognize(extractedText);

        // If nothing recognised,
        // fall back to the specification scanner.
        if (result.Material == "UNKNOWN")
        {
            result.Material =
                _materialRecognition.Recognize(scannedText);
        }

        System.Diagnostics.Debug.WriteLine($"Material : {result.Material}");

        result.PNumber =
            _pNumberRecognition.Recognize(
                result.Material,
                materialText);

        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Material : {result.Material}");
        Console.WriteLine($"P Number : {result.PNumber}");
        Console.WriteLine("========================================");

        System.Diagnostics.Debug.WriteLine($"P Number : {result.PNumber}");
        System.Diagnostics.Debug.WriteLine("========================================");

        return result;
    }
}
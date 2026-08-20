using System.Text.RegularExpressions;
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

        // =========================================================
        // STRUCTURED BASE MATERIAL IDENTIFICATION
        // =========================================================
        //
        // Use the actual material information contained in the PQR.
        // Do NOT use the PQR number to determine material.
        // Do NOT use P-Number as a material selector.
        //
        // UNS is the strongest identifier.
        // Specification and Grade provide additional identification.
        // =========================================================

        var materialUpper =
            materialText.ToUpperInvariant();

        // ---------------------------------------------------------
        // UNS
        // Examples:
        // S31603
        // S31600
        // S32750
        // S32760
        // R50400
        // R56400
        // N06600
        // N06625
        // N08904
        // ---------------------------------------------------------

        var unsMatch =
            Regex.Match(
                materialUpper,
                @"\b([SNR]\d{5})\b",
                RegexOptions.IgnoreCase);

        if (unsMatch.Success)
        {
            result.MaterialUNS =
                unsMatch.Groups[1].Value.ToUpperInvariant();
        }

        // ---------------------------------------------------------
        // Specification
        //
        // Capture common ASME / ASTM material specifications.
        // Examples:
        // SA-240
        // SA 240
        // SB-861
        // SB 861
        // SA-106
        // SA-790
        // SB-163
        // B265
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // Specification + Grade
        //
        // Prefer an explicit MATERIAL SPECIFICATION declaration.
        // This prevents UNS values such as S30403 from being
        // incorrectly interpreted as material specifications.
        //
        // Example:
        // MATERIAL SPECIFICATION: ASME SA312/SA312M 304L
        //
        // Expected:
        // Specification = ASME SA312/SA312M
        // Grade         = 304L
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // Specification + Grade
        //
        // The SpecificationScanner extracts the explicit PQR
        // declaration. OCR may remove the "/" between the two
        // specification designations.
        //
        // Examples received from real OCR:
        //
        // MATERIAL SPECIFICATION: ASME SA312/SA312M 304L
        // MATERIAL SPECIFICATION: ASME SA312 SA312M 304L
        //
        // Both must produce:
        //
        // Specification = ASME SA312/SA312M
        // Grade         = 304L
        // ---------------------------------------------------------

        var materialSpecificationMatch =
            Regex.Match(
                materialUpper,
                @"MATERIAL\s+SPECIFICATION\s*:\s*(ASME\s+)?(?<spec1>(?:SA|SB|A|B)\s*[-]?\s*\d{2,5})(?:\s*(?:/|\s)\s*(?<spec2>(?:SA|SB|A|B)\s*[-]?\s*\d{2,5}M))?\s+(?<grade>304L|316L|310S|[A-Z][A-Z0-9\-]*)",
                RegexOptions.IgnoreCase);

        if (materialSpecificationMatch.Success)
        {
            var specificationPrefix =
                string.IsNullOrWhiteSpace(materialSpecificationMatch.Groups[1].Value)
                    ? ""
                    : "ASME ";

            var spec1 =
                Regex.Replace(
                    materialSpecificationMatch.Groups["spec1"].Value,
                    @"\s+",
                    "")
                .Trim()
                .ToUpperInvariant();

            var spec2 =
                Regex.Replace(
                    materialSpecificationMatch.Groups["spec2"].Value,
                    @"\s+",
                    "")
                .Trim()
                .ToUpperInvariant();

            result.MaterialSpecification =
                specificationPrefix +
                (string.IsNullOrWhiteSpace(spec2)
                    ? spec1
                    : $"{spec1}/{spec2}");

            if (string.IsNullOrWhiteSpace(result.MaterialGrade))
            {
                result.MaterialGrade =
                    materialSpecificationMatch.Groups["grade"].Value
                        .Trim()
                        .ToUpperInvariant();
            }
        }

        // ---------------------------------------------------------
        // Generic specification fallback.
        //
        // Only use this when an explicit material specification
        // declaration was not found.
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(result.MaterialSpecification))
        {
            var specificationMatch =
                Regex.Match(
                    materialUpper,
                    @"\b((?:SA|SB|A|B)[A-Z]?\s*[-]?\s*\d{2,5})\b",
                    RegexOptions.IgnoreCase);

            if (specificationMatch.Success)
            {
                result.MaterialSpecification =
                    Regex.Replace(
                        specificationMatch.Groups[1].Value,
                        @"\s+",
                        " ")
                    .Trim();
            }
        }

        // ---------------------------------------------------------
        // Grade
        //
        // Capture explicit Grade values where available.
        // Examples:
        // Grade 2
        // Grade 5
        // Grade 7
        // Grade A
        // Grade B
        // Grade C
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(result.MaterialGrade))
        {
            var gradeMatch =
                Regex.Match(
                    materialUpper,
                    @"\bGRADE\s*([A-Z0-9][A-Z0-9\-]*)\b",
                    RegexOptions.IgnoreCase);

            if (gradeMatch.Success)
            {
                result.MaterialGrade =
                    gradeMatch.Groups[1].Value.Trim().ToUpperInvariant();
            }
        }

        // ---------------------------------------------------------
        // Common grade forms where the PQR writes the grade
        // directly beside the material designation.
        //
        // Examples:
        // 316L
        // 304L
        // 310S
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(result.MaterialGrade))
        {
            var alloyGradeMatch =
                Regex.Match(
                    materialUpper,
                    @"\b(304L|316L|310S)\b",
                    RegexOptions.IgnoreCase);

            if (alloyGradeMatch.Success)
            {
                result.MaterialGrade =
                    alloyGradeMatch.Groups[1].Value.ToUpperInvariant();
            }
        }

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






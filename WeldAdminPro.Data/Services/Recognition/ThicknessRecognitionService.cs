using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services.Recognition;

public class ThicknessRecognitionService
{    
    public ThicknessRecognitionResult RecognizeWps(
    string text,
    string materialGroup,
    Pqr? linkedPqr)
    {
        var result = new ThicknessRecognitionResult();

        if (linkedPqr != null)
        {
            result.MinimumThickness = linkedPqr.ThicknessQualifiedMin;
            result.MaximumThickness = linkedPqr.ThicknessQualifiedMax;

            return result;
        }

        var thicknessText = text ?? "";

        thicknessText = thicknessText.Replace(",", ".");

        // SAF2507 OCR fixes
        thicknessText = Regex.Replace(
            thicknessText,
            @"\b15\s*[-–]\s*1108\b",
            "1.5-11.08",
            RegexOptions.IgnoreCase);

        thicknessText = Regex.Replace(
            thicknessText,
            @"\b15\s*[-–]\s*1524\b",
            "1.5-15.24",
            RegexOptions.IgnoreCase);

        thicknessText = Regex.Replace(
            thicknessText,
            @"T1\.5\s*-\s*T1524",
            "T1.5-T15.24",
            RegexOptions.IgnoreCase);

        thicknessText = Regex.Replace(
            thicknessText,
            @"T1524\b",
            "T15.24",
            RegexOptions.IgnoreCase);

        thicknessText = Regex.Replace(
            thicknessText,
            @"1\.5\s*-\s*1524\b",
            "1.5-15.24",
            RegexOptions.IgnoreCase);

        TryReadApprovalRange(
            thicknessText,
            result);

        if (result.MaximumThickness <= 0)
        {
            TryReadDesignationRange(
                thicknessText,
                result);
        }

        if (result.MaximumThickness <= 0)
        {
            TryReadFallbackRange(
                thicknessText,
                result);
        }

        

        // =====================================
        // TESTED THICKNESS
        // =====================================

        var testedMatch = Regex.Match(
            thicknessText,
            @"TEST(?:ED)?\s*THICKNESS.*?(\d+[\.,]?\d*)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (testedMatch.Success &&
            double.TryParse(
                testedMatch.Groups[1].Value.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var tested))
        {
            result.TestedThickness = tested;
        }

        if (result.TestedThickness > 0 &&
            result.MaximumThickness <= 0)
        {
            CalculateQualificationRange(
                result,
                result.TestedThickness);
        }

        ApplyMaterialOverrides(
            materialGroup,
            result);

        result.NormalizedText = thicknessText;
                
        return result;
    }

    public ThicknessRecognitionResult RecognizePqr(
    string text)
    {
        var result = new ThicknessRecognitionResult();

        text ??= "";

        text = text.Replace(",", ".");

        // =====================================
        // Tested Thickness
        // =====================================

        // Pattern 1
        // Sch 80S 7.62

        var schInline = Regex.Match(
            text,
            @"Sch\s*\d+\s*[A-Za-z]*\s*(\d+[\.,]?\d*)",
            RegexOptions.IgnoreCase);

        if (schInline.Success)
        {
            result.TestedThickness =
                ParseDouble(schInline.Groups[1].Value);
        }

        // Pattern 2
        // Sch. Thickness (mm) 10

        if (result.TestedThickness <= 0)
        {
            var schDirect = Regex.Match(
                text,
                @"Sch\.?\s*Thickness\s*\(mm\)\s*[:\-]?\s*(\d+[\.,]?\d*)",
                RegexOptions.IgnoreCase);

            if (schDirect.Success)
            {
                result.TestedThickness =
                    ParseDouble(schDirect.Groups[1].Value);
            }
        }

        // Pattern 3
        // Block layout

        if (result.TestedThickness <= 0)
        {
            var schBlock = Regex.Match(
                text,
                @"Sch\.?\s*Thickness\s*\(mm\)(.*?)(Without|Pass|$)",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

            if (schBlock.Success)
            {
                var numbers =
                    Regex.Matches(
                        schBlock.Groups[1].Value,
                        @"\d+[\.,]?\d*")
                    .Select(m => ParseDouble(m.Value))
                    .Where(n => n > 2 && n < 50)
                    .ToList();

                if (numbers.Any())
                {
                    result.TestedThickness =
                        numbers.Max();
                }
            }
        }

        // Pattern 4
        // THICKNESS :5.49mm

        if (result.TestedThickness <= 0)
        {
            var direct = Regex.Match(
                text,
                @"THICKNESS\s*[:\-]?\s*(\d+[\.,]?\d*)\s*mm",
                RegexOptions.IgnoreCase);

            if (direct.Success)
            {
                result.TestedThickness =
                    ParseDouble(direct.Groups[1].Value);
            }
        }

        return result;
    }

    private static void TryReadApprovalRange(
    string thicknessText,
    ThicknessRecognitionResult result)
    {
        var approvalMatch = Regex.Match(
            thicknessText,
            @"APPROVAL\s*RANGE.*?THICKNESS.*?(\d+[\.,]?\d*)\s*[-–]\s*(\d+[\.,]?\d*)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!approvalMatch.Success)
            return;

        var min = double.Parse(
            approvalMatch.Groups[1].Value.Replace(",", "."),
            CultureInfo.InvariantCulture);

        var max = double.Parse(
            approvalMatch.Groups[2].Value.Replace(",", "."),
            CultureInfo.InvariantCulture);

        if (min >= 0.5 &&
            max <= 50 &&
            max > min)
        {
            result.MinimumThickness = min;
            result.MaximumThickness = max;
        }
    }

    private static void TryReadDesignationRange(
    string thicknessText,
    ThicknessRecognitionResult result)
    {
        var designationMatch = Regex.Match(
            thicknessText,
            @"T\s*(\d+[\.,]?\d*)\s*[-–]\s*(\d+[\.,]?\d*)\s*MM",
            RegexOptions.IgnoreCase);

        if (!designationMatch.Success)
            return;

        var min = double.Parse(
            designationMatch.Groups[1].Value.Replace(",", "."),
            CultureInfo.InvariantCulture);

        var max = double.Parse(
            designationMatch.Groups[2].Value.Replace(",", "."),
            CultureInfo.InvariantCulture);

        if (min >= 0.5 &&
            max <= 50 &&
            max > min)
        {
            result.MinimumThickness = min;
            result.MaximumThickness = max;
        }
    }

    private static void CalculateQualificationRange(
    ThicknessRecognitionResult result,
    double testedThickness)
    {
        if (testedThickness <= 0)
            return;

        if (testedThickness <= 1.5)
        {
            result.MinimumThickness = Math.Round(
                0.5 * testedThickness,
                2);

            result.MaximumThickness = Math.Round(
                2 * testedThickness,
                2);
        }
        else
        {
            result.MinimumThickness = 1.5;

            result.MaximumThickness = Math.Round(
                2 * testedThickness,
                2);
        }
    }

    private (double min, double max) ExtractTestThickness(string text)
    {
        text = text.Replace(",", ".");

        // 🔥 STEP 1: STRONG FILTER → ONLY THICKNESS CONTEXT
        var matches = Regex.Matches(text,
            @"THICKNESS[^0-9]{0,20}(\d+[\.,]?\d*)\s*[-–TO]{1,3}\s*(\d+[\.,]?\d*)",
            RegexOptions.IgnoreCase);


        double bestMin = 0;
        double bestMax = 0;

        foreach (Match m in matches)
        {
            var tMin = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var tMax = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);

            // 🔥 VALID RANGE FILTER
            if (tMin >= 0.5 && tMax <= 50)
            {
                // 🔥 PICK THE LARGEST VALID RANGE (PQR logic)
                if (tMax > bestMax)
                {
                    bestMin = tMin;
                    bestMax = tMax;
                }
            }
        }

        if (bestMax > 0)
            return (bestMin, bestMax);

        // 🔥 FALLBACK → SINGLE VALUE (ONLY IF CLEAR)
        var singleMatches = Regex.Matches(text,
            @"THICKNESS[^0-9]{0,20}(\d+[\.,]?\d*)",
            RegexOptions.IgnoreCase);

        foreach (Match m in singleMatches)
        {
            var val = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);

            if (val >= 1 && val <= 50)
            {
                return (1.5, val * 2);
            }
        }

        return (0, 0);
    }

    private void TryReadFallbackRange(
    string thicknessText,
    ThicknessRecognitionResult result)
    {
        var range = ExtractTestThickness(thicknessText);

        result.MinimumThickness = range.min;
        result.MaximumThickness = range.max;
    }

    private static void ApplyMaterialOverrides(
    string materialGroup,
    ThicknessRecognitionResult result)
    {
        if (materialGroup.Contains("904"))
        {
            result.MinimumThickness = 1.5;
            result.MaximumThickness = 7.82;
        }

        if (materialGroup.Contains("Titanium"))
        {
            result.MinimumThickness = 1.5;
            result.MaximumThickness = 7.82;
        }
    }

    private static double ParseDouble(string value)
    {
        value = value.Replace(",", ".");

        double.TryParse(
            value,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var result);

        return result;
    }

}


public class ThicknessRecognitionResult
{
    public double TestedThickness { get; set; }

    public double MinimumThickness { get; set; }

    public double MaximumThickness { get; set; }

    public string NormalizedText { get; set; } = "";
}
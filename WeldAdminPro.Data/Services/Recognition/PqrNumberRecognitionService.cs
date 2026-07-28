using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class PqrNumberRecognitionService
{
    public string Recognize(string text, string fallbackName)
    {
        text ??= "";
        fallbackName ??= "";

        Match match;

        // -----------------------------------------
        // 1. Standard PQR Number (Highest Priority)
        // -----------------------------------------
        match = Regex.Match(
            text,
            @"PQR\s*(?:NUMBER|NO\.?)\s*[:\-]?\s*([A-Z0-9\/\-]+(?:\s+[A-Z0-9\/\-]+){0,2})",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return Clean(match.Groups[1].Value);

        // -----------------------------------------
        // 2. DYN-PQR-xxxx
        // -----------------------------------------
        match = Regex.Match(
            text,
            @"DYN[-/]PQR[-/][A-Z0-9\-]{4,}",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return Clean(match.Value);

        // -----------------------------------------
        // 3. PQ5418-P1 REV 1
        // -----------------------------------------
        match = Regex.Match(
            text,
            @"PQ\d+(?:-[A-Z0-9]+)*(?:\s+REV\s+\d+)?",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return Clean(match.Value);

        // -----------------------------------------
        // 4. Supplier PQR
        // -----------------------------------------
        match = Regex.Match(
            text,
            @"SUPP\.?\s*PQR\s*NO\.?\s*[:\-]?\s*(\d+)",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return match.Groups[1].Value.Trim();

        // -----------------------------------------
        // 5. Internal PQR ID (Lowest Priority)
        // -----------------------------------------
        match = Regex.Match(
            text,
            @"PQR\s*ID\s*[:\-]?\s*([A-Z0-9\/\-]+)",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return Clean(match.Groups[1].Value);

        // -----------------------------------------
        // 6. Fallback
        // -----------------------------------------
        return Clean(fallbackName);
    }

    private static string Clean(string value)
    {
        value = Regex.Replace(value ?? "", @"\s+", " ").Trim();

        // remove trailing dash
        value = value.TrimEnd('-');

        // remove trailing .PDF
        value = Regex.Replace(value, @"\.PDF$", "", RegexOptions.IgnoreCase);

        value = Regex.Replace(
            value,
            @"^PQRSA",
            "PQR SA",
            RegexOptions.IgnoreCase);

        value = Regex.Replace(
            value,
            @"\.PDF.*$",
            "",
            RegexOptions.IgnoreCase);

        // Remove OCR revision suffixes
        value = Regex.Replace(
            value,
            @"\s+REV\s*/?.*$",
            "",
            RegexOptions.IgnoreCase);

        value = Regex.Replace(
            value,
            @"\s+REVISION.*$",
            "",
            RegexOptions.IgnoreCase);

        value = value.Trim();

        return value;
    }
}
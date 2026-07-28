using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class TextNormalizationService
{
    public string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Existing fixes
        text = text.Replace("$", "S");
        text = text.Replace("5A", "SA");

        // -----------------------------
        // OCR fixes
        // -----------------------------

        // A310 -> SA310
        text = Regex.Replace(
            text,
            @"\bA\s*(3\d{2})\b",
            "SA$1",
            RegexOptions.IgnoreCase);

        // 531008 -> S31008
        text = Regex.Replace(
            text,
            @"\b5(3\d{4})\b",
            "S$1",
            RegexOptions.IgnoreCase);

        // 530403 -> S30403
        text = Regex.Replace(
            text,
            @"\b5(30\d{3})\b",
            "S$1",
            RegexOptions.IgnoreCase);

        // SA 310 -> SA310
        text = Regex.Replace(
            text,
            @"\bSA\s+(\d+)\b",
            "SA$1",
            RegexOptions.IgnoreCase);

        // TP 310 -> TP310
        text = Regex.Replace(
            text,
            @"\bTP\s+(\d+)\b",
            "TP$1",
            RegexOptions.IgnoreCase);

        // Collapse punctuation
        text = Regex.Replace(text, @"[\-_/]", " ");
        text = Regex.Replace(text, @"\s+", " ");

        return text.Trim();
    }
}
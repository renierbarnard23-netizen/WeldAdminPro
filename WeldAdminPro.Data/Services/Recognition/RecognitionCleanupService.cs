using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public static class RecognitionCleanupService
{
    private static readonly HashSet<string> InvalidValues =
    [
        "",
        "PQR Number",
        "WPS Number",
        "Code/Standard",
        "Designation",
        "Revision",
        "Date",
        "Rev",
        "Rev/ Ver",
        "Construction Code",
        "Constr. Code"
    ];

    public static string Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.Trim();

        // Normalize whitespace
        value = Regex.Replace(value, @"\s+", " ");

        // Remove trailing OCR punctuation
        value = value.TrimEnd('-', '/', ':', ';', '.', ',');

        // Remove surrounding brackets
        value = value.Trim('(', ')', '[', ']');

        return value;
    }

    public static bool IsValid(string? value)
    {
        value = Clean(value);

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length < 3)
            return false;

        return !InvalidValues.Contains(value);
    }
}
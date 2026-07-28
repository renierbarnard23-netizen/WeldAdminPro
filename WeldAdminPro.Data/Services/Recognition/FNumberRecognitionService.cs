using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class FNumberRecognitionService
{
    public string Recognize(
        string text,
        string materialText,
        string materialGroup)
    {
        text ??= "";
        materialText ??= "";
        materialGroup ??= "";

        // =========================
        // Locate filler section
        // =========================

        var fillerSection = Regex.Match(
            text,
            @"FILLER\s*METALS?.{0,2000}",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var fillerText =
            fillerSection.Success
                ? fillerSection.Value
                : text;

        // =========================
        // Filler recognition
        // =========================

        var matches = Regex.Matches(
            fillerText,

            @"\bE\s*R\s*\d\s*\d\s*\d\s*[A-Z0-9\-]*\b" +
            @"|\bER\s*\d{2,4}[A-Z0-9\-]*\b" +
            @"|\bE\s*7\s*0\s*1\s*8\b" +
            @"|\bE\s*R\s*T\s*I[-\s]?\d+\b" +
            @"|\bERTI[-\s]?\d+\b" +
            @"|\bE\s*R\s*N\s*I[A-Z0-9\-]*\b" +
            @"|\bERNI[A-Z0-9\-]*\b" +
            @"|\bINCONEL\b",

            RegexOptions.IgnoreCase);

        foreach (Match m in matches)
        {
            var val =
                Regex.Replace(
                    m.Value.ToUpper(),
                    @"\s+",
                    "");

            if (val.Contains("ERTI"))
                return "F51";

            if (val.Contains("ERNI") ||
                val.Contains("NICR") ||
                val.Contains("INCONEL"))
                return "F41";

            if (val.Contains("308") ||
                val.Contains("316") ||
                val.Contains("2594"))
                return "F6";
        }

        // =========================
        // Material fallbacks
        // =========================

        var mat = materialGroup.ToUpperInvariant();

        if (mat.Contains("304") ||
            mat.Contains("310") ||
            mat.Contains("316"))
            return "F6";

        if (mat.Contains("430") ||
            mat.Contains("A106") ||
            mat.Contains("CARBON"))
            return "F6";

        if (mat.Contains("904"))
            return "F41";

        if (mat.Contains("2507") ||
            mat.Contains("DUPLEX"))
            return "F6";

        if (mat.Contains("NICKEL") ||
            materialText.Contains("SB163"))
            return "F41";

        if (mat.Contains("TITANIUM"))
            return "F51";

        // Carbon consumables

        if (Regex.IsMatch(text,
            @"E7018|ER70S6|ER70",
            RegexOptions.IgnoreCase))
            return "F4";

        return "";
    }
}
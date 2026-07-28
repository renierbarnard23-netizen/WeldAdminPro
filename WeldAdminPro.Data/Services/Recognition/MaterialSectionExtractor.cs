using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class MaterialSectionExtractor
{
    public string Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var match = Regex.Match(
            text,
            @"(BASE MATERIALS?|BASE METALS?|PARENT MATERIAL).*?(FILLER|WELDING|PREHEAT|PROCESS|POSITION)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        string materialText;

        if (match.Success && match.Value.Length > 50)
        {
            materialText = match.Value;
        }
        else
        {
            match = Regex.Match(
                text,
                @"BASE\s+METALS?.*?(?=WELDING\s+DATA|FILLER\s+METALS|PREHEAT|GAS\s*\(QW-408\)|TECHNIQUE\s*\(QW-410\)|ELECTRICAL\s*\(QW-409\)|TEST RESULTS|$)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (match.Success)
            {
                materialText = match.Value;
            }
            else
            {
                materialText = text.Substring(0, Math.Min(400, text.Length));
            }
        }

        materialText = Regex.Replace(
            materialText,
            @"WPS[-\s]*SAF\s*2507\s*(REV|DATE)",
            "",
            RegexOptions.IgnoreCase);

        return materialText.ToUpperInvariant();
    }
}
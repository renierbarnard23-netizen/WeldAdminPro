using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class MaterialRecognitionService
{
    public string Recognize(string materialText)
    {
        if (string.IsNullOrWhiteSpace(materialText))
            return "UNKNOWN";

        materialText = materialText.ToUpperInvariant();

        // Titanium
        if (Regex.IsMatch(materialText,
            @"R50400|GR\s*2|TITANIUM",
            RegexOptions.IgnoreCase))
            return "Titanium";

        // 904L
        if (Regex.IsMatch(materialText,
            @"904L|N08904|UNS\s*N08904|ALLOY\s*904L",
            RegexOptions.IgnoreCase))
            return "Nickel Alloy 904L";

        // Nickel
        if (Regex.IsMatch(materialText,
            @"SB[\s\-]*163|N06600|N06625|INCONEL",
            RegexOptions.IgnoreCase))
            return "Nickel Alloy";

        // Duplex (must have a real Duplex identifier)
        if (Regex.IsMatch(materialText,
            @"A[/\s-]*SA[/\s-]*790.*S32[57]50|
          UNS\s*S32[57]50|
          SAF\s*2507|
          S32750|
          S32550",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
            return "Duplex SAF2507";

        // Carbon
        if (Regex.IsMatch(materialText,
            @"430A|BS\s*1501",
            RegexOptions.IgnoreCase))
            return "Carbon Steel 430A";

        if (Regex.IsMatch(materialText,
            @"(ASTM\s*)?S?A[\s\-]*106(B)?",
            RegexOptions.IgnoreCase))
            return "Carbon Steel A106";

        // Stainless
        if (Regex.IsMatch(materialText, @"TP\s*304L|S30403"))
            return "Stainless 304L";

        if (Regex.IsMatch(materialText, @"TP\s*310|S31000|S31008|SA\s*310|310S"))
            return "Stainless 310";

        if (Regex.IsMatch(materialText, @"TP\s*316|S31600"))
            return "Stainless 316";

        // OCR tolerant
        if (Regex.IsMatch(materialText, @"3\s*0\s*4\s*L"))
            return "Stainless 304L";

        if (Regex.IsMatch(materialText, @"3\s*1\s*0"))
            return "Stainless 310";

        if (Regex.IsMatch(materialText, @"3\s*1\s*6\s*L?"))
            return "Stainless 316";        

        return "UNKNOWN";
    }
}
using System.Text;

namespace WeldAdminPro.Data.Services.Recognition;

public class SmartMaterialExtractor
{
    public string Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var upper = text.ToUpperInvariant();

        int start = upper.IndexOf("BASE METALS");

        if (start < 0)
            return text;

        int end = upper.IndexOf("WELDING DATA", start);

        if (end < 0)
            end = upper.IndexOf("QW-400", start);

        if (end < 0)
            end = upper.IndexOf("FILLER METALS", start);

        if (end < 0)
            end = text.Length;

        return text.Substring(start, end - start);
    }
}
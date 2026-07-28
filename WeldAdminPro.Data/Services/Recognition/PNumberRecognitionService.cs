namespace WeldAdminPro.Data.Services.Recognition;

public class PNumberRecognitionService
{
    public string Recognize(string materialGroup, string materialText)
    {
        var mat = (materialGroup ?? "").ToUpperInvariant();
        materialText = (materialText ?? "").ToUpperInvariant();

        // Titanium
        if (mat.Contains("TITANIUM") || mat.Contains("R50400"))
            return "P51";

        // Duplex
        if (mat.Contains("2507"))
            return "P10H";

        // 904L
        if (mat.Contains("904"))
            return "P45";

        // Nickel alloys (SB163, N06600, N06625, etc.)
        if (materialText.Contains("SB163") ||
            materialText.Contains("N06600") ||
            materialText.Contains("N06625"))
            return "P41";

        // Generic Nickel
        if (mat.Contains("NICKEL"))
            return "P41";

        // Stainless
        if (mat.Contains("304") ||
            mat.Contains("310") ||
            mat.Contains("316"))
            return "P8";

        // Carbon Steel
        if (mat.Contains("430") ||
            mat.Contains("A106"))
            return "P1";

        return "";
    }
}
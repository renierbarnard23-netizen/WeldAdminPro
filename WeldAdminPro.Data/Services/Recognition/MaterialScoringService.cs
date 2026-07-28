using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class MaterialScoringService
{
    public string DetermineBest(string materialText)
    {
        var scores = new Dictionary<string, int>();

        void AddScore(string material, int value)
        {
            if (!scores.ContainsKey(material))
                scores[material] = 0;

            scores[material] += value;
        }

        // Carbon Steel
        if (Regex.IsMatch(materialText, @"430A|BS\s*1501|A105|A106"))
            AddScore("Carbon Steel", 5);

        if (Regex.IsMatch(materialText, @"A106"))
            AddScore("Carbon Steel A106", 10);

        // Stainless
        if (Regex.IsMatch(materialText, @"3\s*0\s*4\s*L"))
            AddScore("Stainless 304L", 20);

        if (Regex.IsMatch(materialText, @"3\s*1\s*6\s*L?"))
            AddScore("Stainless 316", 20);

        if (Regex.IsMatch(materialText, @"3\s*1\s*0"))
            AddScore("Stainless 310", 20);

        // Duplex
        if (Regex.IsMatch(materialText, @"S32750") &&
            Regex.IsMatch(materialText, @"A/SA[-\s]*790"))
            AddScore("Duplex SAF2507", 20);

        // Nickel
        if (Regex.IsMatch(materialText, @"904L|N08904"))
            AddScore("Nickel Alloy 904L", 15);

        if (Regex.IsMatch(materialText, @"SB\s*163"))
            AddScore("Nickel Alloy", 50);

        // Titanium
        if (Regex.IsMatch(materialText,
            @"R50400|GR\s*2|TITANIUM",
            RegexOptions.IgnoreCase))
            AddScore("Titanium", 15);

        if (scores.Count == 0)
            return "UNKNOWN";

        var best = scores
            .OrderByDescending(x => x.Value)
            .First()
            .Key;

        return best switch
        {
            "Carbon Steel" => "Carbon Steel 430A",
            _ => best
        };
    }
}
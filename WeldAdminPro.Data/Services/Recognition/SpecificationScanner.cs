using System.Text;
using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Services.Recognition;

public class SpecificationScanner
{
    public string Scan(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        // Ignore attachment pages.
        // Scan only the first page to avoid picking up
        // material references from radiography reports.
        var page2 = Regex.Match(
            text,
            @"Page\s+2\s+of",
            RegexOptions.IgnoreCase);

        if (page2.Success)
        {
            text = text.Substring(0, page2.Index);
        }

        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Find(matches, text, @"S30403|TP\s*304L");
        Find(matches, text, @"S31000|S31008|TP\s*310");
        Find(matches, text, @"S31600|S31603|TP\s*316");
        Find(matches, text, @"SA\s*106|ASTM\s*A106");
        Find(matches, text, @"430A|BS\s*1501");
        Find(matches, text, @"N08904");
        Find(matches, text, @"SB\s*163|N06625|N06600");
        Find(matches, text, @"S32750|S32550|SAF\s*2507|2507|A\/?SA[-\s]*790");
        Find(matches, text, @"R50400|SB\s*861");

        return string.Join(Environment.NewLine, matches);
    }
    private static void Find(
    HashSet<string> matches,
    string text,
    string pattern)
    {
        foreach (Match match in Regex.Matches(
                     text,
                     pattern,
                     RegexOptions.IgnoreCase))
        {
            matches.Add(match.Value.Trim());
        }
    }
}
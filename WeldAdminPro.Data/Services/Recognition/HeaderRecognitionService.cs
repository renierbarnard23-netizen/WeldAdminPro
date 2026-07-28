using System.Collections.Generic;
using System.Text.RegularExpressions;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services.Recognition;

public class HeaderRecognitionService
{
    public DocumentHeader Recognize(string text)
    {
        text ??= string.Empty;

        return new DocumentHeader
        {
            PqrNumber = ReadPqrNumber(text),
            WpsNumber = ReadWpsNumber(text),
            CodeStandard = ReadCodeStandard(text),
            Revision = ReadRevision(text),
            Date = ReadDate(text)
        };
    }

    private static string ReadPqrNumber(string text)
    {
        text ??= string.Empty;

        var value = FindBestMatch(
            text,

            @"PQR\s*Number[:\s]+(PQR\s*SA[0-9A-Z]+)",

            @"PQR\s*Number[:\s]+(DYN[/\-]PQR[/\-][A-Z0-9\-]+)",

            @"\b(PQ\d{4,}-P\d+)\b",

            @"\b(DYN[-\s]*\d{2}[A-Z])\b",

            @"PQR\s*SB\s*\d+",

            @"PQR\s*ID[:\s]+(DYN[/\-]PQR[/\-][A-Z0-9\-]+)",

            @"PQR\s*ID[:\s]+(PQRSAF[0-9A-Z]+)",

            @"DYN[/\-]PQR[/\-][A-Z0-9\-]+",

            @"PQR\s+SA[0-9A-Z]+",

            @"PQRSAF[0-9A-Z]+",

            @"SUPP\.?\s*PQR\s*NO\.?\s*:?\s*(\d+)"
        );

        return value;
    }

    private static string ReadWpsNumber(string text)
    {
        text ??= string.Empty;

        var value = FindBestMatch(
            text,

            @"WPS\s*ID[:\s]+(DYN[/\-]WPS[/\-][A-Z0-9\-]+)",

            @"WPS\s*ID[:\s]+(WPSSAF[0-9A-Z]+)",

            @"\b(WPS\d{4,}-W\d+)\b",

            @"WPS\s*SB\s*\d+",

            @"DYN[/\-]WPS[/\-][A-Z0-9\-]+",

            @"\b(PWPS[-\s]*DYN[-\s]*\d{2})\b",

            @"WPS\s*SA[0-9A-Z]+",

            @"WPSSAF[0-9A-Z]+",

            @"PWPS[-\s]*SAF[0-9A-Z]+"
        );

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return ReadField(
            text,
            "WPS Number",
            "Code/Standard",
            "Construction Code");
    }

    private static string ReadCodeStandard(string text)
    {
        text ??= string.Empty;

        var patterns = new[]
        {
        @"PQR\s+ASME\s+BPVC\s+Sec\.\s*IX\s*-\s*\d{4}",
        @"ASME\s+BPVC\s+Sec\.\s*IX\s*-\s*\d{4}",
        @"ASME\s+IX\s*&\s*VIII\s+Div\s*1"
    };

        return FindBestMatch(text, patterns);
    }

    private static string ReadRevision(string text)
    {
        text ??= string.Empty;

        var match = Regex.Match(
            text,
            @"Rev(?:ision)?/?\s*Ver\s*\[?(\d+)\]?|Rev(?:ision)?\s*:?\s*(\d+)",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return string.Empty;

        foreach (Group group in match.Groups)
        {
            if (group.Success && int.TryParse(group.Value, out _))
                return group.Value;
        }

        return string.Empty;
    }

    private static string ReadDate(string text)
    {
        text ??= string.Empty;

        var patterns = new[]
        {
        // 05/05/2025
        @"\b\d{2}/\d{2}/\d{4}\b",

        // 8-Nov-2023
        @"\b\d{1,2}-[A-Za-z]{3}-\d{4}\b",

        // 2023/10/16
        @"\b\d{4}/\d{2}/\d{2}\b"
    };

        return FindBestMatch(text, patterns);
    }

    private static string FindBestMatch(string text, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var value = match.Groups.Count > 1
                    ? match.Groups[1].Value
                    : match.Value;

                value = RecognitionCleanupService.Clean(value);

                if (!RecognitionCleanupService.IsValid(value))
                    continue;

                return value;
            }
        }

        return string.Empty;
    }

    private static string ReadField(
    string text,
    string startLabel,
    params string[] endLabels)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var start = Regex.Match(
            text,
            Regex.Escape(startLabel),
            RegexOptions.IgnoreCase);

        if (!start.Success)
            return string.Empty;

        var value = text[(start.Index + start.Length)..];

        var stopWords = new List<string>();

        stopWords.AddRange(endLabels);

        stopWords.AddRange(new[]
        {
        "Rev/ Ver",
        "Rev",
        "Revision",
        "Constr. Code",
        "Construction Code",
        "JOINT DESIGN",
        "JOINT DESIGN (QW-402)",
        "BASE METALS",
        "WELDING DATA"
    });

        var end = value.Length;

        foreach (var stop in stopWords)
        {
            var m = Regex.Match(
                value,
                Regex.Escape(stop),
                RegexOptions.IgnoreCase);

            if (m.Success && m.Index < end)
                end = m.Index;
        }

        value = value[..end];

        value = Regex.Replace(value, @"\s+", " ");

        return value.Trim();
    }
}
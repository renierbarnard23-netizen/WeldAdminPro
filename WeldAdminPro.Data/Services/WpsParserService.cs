using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Services.Recognition;
using static System.Net.Mime.MediaTypeNames;

namespace WeldAdminPro.Data.Services
{
    public class WpsParserService
    {

        private readonly PNumberRecognitionService _pNumberRecognition;
        private readonly MaterialRecognitionService _materialRecognition;
        private readonly MaterialSectionExtractor _materialSectionExtractor;
        private readonly TextNormalizationService _textNormalization;
        private readonly FNumberRecognitionService _fNumberRecognition;
        private readonly ThicknessRecognitionService _thicknessRecognition;

        public WpsParserService(
            PNumberRecognitionService pNumberRecognition,
            MaterialRecognitionService materialRecognition,
            MaterialSectionExtractor materialSectionExtractor,
            TextNormalizationService textNormalization,
            FNumberRecognitionService fNumberRecognition,
            ThicknessRecognitionService thicknessRecognition)
        {
            _pNumberRecognition = pNumberRecognition;
            _materialRecognition = materialRecognition;
            _materialSectionExtractor = materialSectionExtractor;
            _textNormalization = textNormalization;
            _fNumberRecognition = fNumberRecognition;
            _thicknessRecognition = thicknessRecognition;
        }
        public List<Wps> ParseMultiple(string text, string fallbackName, Pqr? linkedPqr = null)
        {
            return new List<Wps> { ParseSingle(text, fallbackName, linkedPqr) };
        }

        private Wps ParseSingle(string text, string fallbackName, Pqr? linkedPqr = null)
        {
            text ??= "";

            System.IO.File.WriteAllText($@"C:\Temp\WPS_DEBUG_{DateTime.Now.Ticks}.txt", text);

            var normalizedText =
                NormalizeText(text);

            var lines =
                normalizedText
                    .Split(
                        new[]
                        {
                '\r',
                '\n'
                        },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => NormalizeLine(x))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

            var wps = new Wps
            {
                Id = Guid.NewGuid(),
                WpsNumber = Clean(System.IO.Path.GetFileNameWithoutExtension(fallbackName)),
                Process = text.Contains("GTAW", StringComparison.OrdinalIgnoreCase) ? "TIG" : "UNKNOWN",
                MaterialGroup = "",
                Position = "ALL",
                JointType = "",
                Diameter = 0
            };

            double ParseDouble(string v)
            {
                v = v.Replace(",", ".");
                double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var r);
                return r;
            }

            // =========================
            // CLEAN PRIMARY WPS NUMBER
            // =========================

            var primaryWpsMatch = Regex.Match(
                text,
                @"WPS\s*Number\s*[:#]?\s*([^\r\n]+)",
                RegexOptions.IgnoreCase);

            if (primaryWpsMatch.Success)
            {
                var raw = primaryWpsMatch.Groups[1].Value;

                raw = Regex.Replace(
                    raw,
                    @"\bREV.*",
                    "",
                    RegexOptions.IgnoreCase);

                raw = Regex.Replace(
                    raw,
                    @"\bDATE.*",
                    "",
                    RegexOptions.IgnoreCase);

                raw = raw.Replace(".PDF", "", StringComparison.OrdinalIgnoreCase);

                raw = raw.Trim();

                // OCR fixes
                raw = raw.Replace("5A", "SA");

                wps.WpsNumber = raw;
            }

            // =========================
            // PQR NUMBER
            // =========================

            // Match:
            // PQR Number : PQR-SA310
            // PQR Number PQR-SA310
            // PQR-SA310

            var pqrMatch = Regex.Match(
                text,
                @"PQR\s*Number\s*[:#]?\s*(PQR[-\s]*[A-Z0-9\-\/]+)",
                RegexOptions.IgnoreCase);

            if (!pqrMatch.Success)
            {
                pqrMatch = Regex.Match(
                    text,
                    @"\b(PQR[-\s]*[A-Z0-9\-\/]+)\b",
                    RegexOptions.IgnoreCase);
            }

            if (pqrMatch.Success)
            {
                var pqr = pqrMatch.Groups[1].Value.Trim();

                pqr = pqr.Replace(" ", "-");

                if (!pqr.StartsWith("PQR-", StringComparison.OrdinalIgnoreCase))
                    pqr = "PQR-" + pqr.Replace("PQR", "").Trim('-');

                wps.LinkedPqrNumber = pqr;
            }


            // FINAL SAFETY
            //if (string.IsNullOrWhiteSpace(wps.WpsNumber))
            //{
            //  wps.WpsNumber = $"{Clean(fallbackName)}-{DateTime.Now.Ticks}";
            //}

            // =========================
            // ✅ HEADER SECTION ONLY
            // =========================

            var header = text.Substring(0, Math.Min(400, text.Length));

            // =========================
            // ✅ P NUMBER
            // =========================

            var pMatch = Regex.Match(header, @"P\s*([0-9]{1,2}[A-Z]?)", RegexOptions.IgnoreCase);
            if (pMatch.Success)
                wps.PNumber = $"P{pMatch.Groups[1].Value.ToUpper()}";


            // =========================
            // MATERIAL SECTION
            // =========================

            var materialText =
                _materialSectionExtractor.Extract(text);

            materialText =
                _textNormalization.Normalize(materialText);

            // Normalize spacing
            materialText = Regex.Replace(materialText, @"[\-_/]", " ");
            materialText = Regex.Replace(materialText, @"\s+", " ");

            // =========================
            // MATERIAL RECOGNITION
            // =========================

            wps.MaterialGroup =
                _materialRecognition.Recognize(materialText);


            // =========================
            // 🔥 FINAL THICKNESS LOGIC
            // =========================

            var thickness =
                _thicknessRecognition.RecognizeWps(
                    text,
                    wps.MaterialGroup,
                    linkedPqr);

            wps.ThicknessMin = thickness.MinimumThickness;
            wps.ThicknessMax = thickness.MaximumThickness;

            // =========================
            // ✅ P FROM MATERIAL
            // =========================

            // 🔥 FORCE OVERRIDE FROM MATERIAL (FINAL FIX)
            wps.PNumber =
                _pNumberRecognition.Recognize(
                    wps.MaterialGroup,
                    materialText);

            // =========================
            // F NUMBER
            // =========================

            wps.FNumber =
                _fNumberRecognition.Recognize(
                    text,
                    materialText,
                    wps.MaterialGroup);

            // =========================
            // ✅ POSITION (FIXED)
            // =========================

            var positionSection =
                ExtractApprovalSection(text);

            if (Regex.IsMatch(positionSection, @"\b6G\b", RegexOptions.IgnoreCase))
            {
                wps.Position = "6G";
            }
            else if (Regex.IsMatch(positionSection, @"ANY", RegexOptions.IgnoreCase))
            {
                wps.Position = "ALL";
            }
            else if (Regex.IsMatch(positionSection, @"4G", RegexOptions.IgnoreCase))
            {
                wps.Position = "4G";
            }
            else
            {
                wps.Position = "";
            }

            // =========================
            // ✅ PWHT
            // =========================

            if (Regex.IsMatch(text, @"WITH\s+PWHT", RegexOptions.IgnoreCase))
            {
                wps.PwhtRequired = true;
            }
            else
            {
                wps.PwhtRequired = false;
            }

            // =========================
            // 🔥 JOINT DETECTION (FINAL)
            // =========================

            var jointText = text;

            // GROOVE / BUTT (same category)
            if (Regex.IsMatch(jointText, @"GROOVE|BUTT", RegexOptions.IgnoreCase))
            {
                wps.JointType = "Groove";
            }
            else if (Regex.IsMatch(jointText, @"FILLET", RegexOptions.IgnoreCase))
            {
                wps.JointType = "Fillet";
            }
            else if (Regex.IsMatch(jointText, @"SOCKET", RegexOptions.IgnoreCase))
            {
                wps.JointType = "Socket";
            }
            else if (Regex.IsMatch(jointText, @"LAP", RegexOptions.IgnoreCase))
            {
                wps.JointType = "Lap";
            }
            else
            {
                wps.JointType = "Groove"; // 🔥 SAFE DEFAULT
            }

            bool isPipe = Regex.IsMatch(text, @"PIPE|DN\s*\d+|NPS|OUTSIDE DIAMETER|OD", RegexOptions.IgnoreCase);
            bool isPlate = Regex.IsMatch(text, @"\bPLATE\b|\bSHEET\b", RegexOptions.IgnoreCase);

            // 🔥 ONLY treat as plate if NOT pipe
            bool isDefinitelyPlate = isPlate && !isPipe;

            if (isDefinitelyPlate)
            {
                wps.Diameter = 0;
            }

            // =========================
            // 🔥 DIAMETER DETECTION (FINAL CLEAN)
            // =========================

            string diaText = "";

            var pipeSection = Regex.Match(text,
                @"OUTSIDE DIAMETER.*?(NO LIMIT|ALL PRACTICAL PURPOSES|ALL DIAMETER|DN\s*\d+.*?DN\s*\d+|\d{2,4}\s*MM\s*[-–TO]{1,3}\s*\d{2,4}\s*MM)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (pipeSection.Success)
            {
                diaText = pipeSection.Value;
            }
            else
            {
                diaText = text; // fallback ONLY if not found
            }

            diaText = diaText.Replace(",", ".");

            // ✅ DECLARE FIRST (GLOBAL TO THIS BLOCK)
            double detectedDiameter = 0;

            // 🔥 SINGLE MATCH MUST BE DECLARED HERE
            var singleMatch = Regex.Match(diaText,
                    @"(OD|OUTSIDE DIAMETER)[^\d]*(\d{2,4})\s*MM",
                    RegexOptions.IgnoreCase);

            // 1️⃣ NO LIMIT
            if (Regex.IsMatch(diaText, @"NO\s*LIMIT|ALL\s*DIAMETER|ALL\s*PRACTICAL", RegexOptions.IgnoreCase))
            {
                detectedDiameter = double.MaxValue;
            }
            else
            {
                // 2️⃣ RANGE
                var rangeMatch = Regex.Match(diaText,
                    @"(\d{2,4})\s*(MM)?\s*[-–TO]{1,3}\s*(\d{2,4})\s*(MM)?",
                    RegexOptions.IgnoreCase);

                if (rangeMatch.Success)
                {
                    var maxDia = ParseDouble(rangeMatch.Groups[3].Value);

                    // 🔥 FIX: reject OCR garbage
                    if (maxDia > 1000)
                        detectedDiameter = 0;
                    else if (maxDia >= 20 && maxDia <= 5000)
                        detectedDiameter = maxDia;
                }
                else
                {
                    // 3️⃣ DN
                    var dnMatch = Regex.Match(diaText,
                        @"DN\s*(\d{2,4}).*?DN\s*(\d{2,4})",
                        RegexOptions.IgnoreCase);

                    if (dnMatch.Success)
                    {
                        var maxDia = ParseDouble(dnMatch.Groups[2].Value);

                        if (maxDia >= 20)
                            detectedDiameter = maxDia;
                    }
                    else
                    {
                        // 4️⃣ SINGLE VALUE
                        if (singleMatch.Success)
                        {
                            var val = ParseDouble(singleMatch.Groups[2].Value);

                            if (val >= 20 && val <= 5000)
                                detectedDiameter = val;
                        }
                    }

                }

            }

            // ❌ Reject unrealistic diameters (like thickness leaks)
            if (detectedDiameter > 0 && detectedDiameter < 20)
            {
                detectedDiameter = 0;
            }

            // ✅ APPLY ONCE
            if (detectedDiameter >= 20 && detectedDiameter <= 5000)
            {
                wps.Diameter = detectedDiameter;
            }

            // 🔥 FINAL PIPE FALLBACK (CORRECT LOCATION)
            if (wps.Diameter == 0 && isPipe)
            {
                wps.Diameter = double.MaxValue;
            }

            Console.WriteLine("===== WPS PARSED =====");
            Console.WriteLine($"Number      : {wps.WpsNumber}");
            Console.WriteLine($"Material    : {wps.MaterialGroup}");
            Console.WriteLine($"P Number    : {wps.PNumber}");
            Console.WriteLine($"F Number    : {wps.FNumber}");
            Console.WriteLine($"Thickness   : {wps.ThicknessMin} - {wps.ThicknessMax}");
            Console.WriteLine("======================");

            return wps;
        }

        private string ExtractSection(
    string text,
    string start,
    string end)
        {
            var match = Regex.Match(
                text,
                $"{start}(.{{0,4000}}?){end}",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

            return match.Success
                ? match.Value
                : "";
        }

        // =========================
        // 🔥 EXTRACT TEST THICKNESS (CRITICAL)
        // =========================

        private (double min, double max)? ExtractFromApproval(string text)
        {
            var match = Regex.Match(text,
                @"THICKNESS[^0-9]{0,20}(\d+[\.,]?\d*)\s*[-–TO]{1,3}\s*(\d+[\.,]?\d*)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            double min = double.Parse(match.Groups[1].Value.Replace(",", "."), CultureInfo.InvariantCulture);
            double max = double.Parse(match.Groups[2].Value.Replace(",", "."), CultureInfo.InvariantCulture);

            if (min >= 1 && max <= 50 && max > min)
                return (min, max);

            return null;
        }

        private (double min, double max) ExtractTestThickness(string text)
        {
            text = text.Replace(",", ".");

            // 🔥 STEP 1: STRONG FILTER → ONLY THICKNESS CONTEXT
            var matches = Regex.Matches(text,
                @"THICKNESS[^0-9]{0,20}(\d+[\.,]?\d*)\s*[-–TO]{1,3}\s*(\d+[\.,]?\d*)",
                RegexOptions.IgnoreCase);
                       

            double bestMin = 0;
            double bestMax = 0;

            foreach (Match m in matches)
            {
                var tMin = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                var tMax = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);

                // 🔥 VALID RANGE FILTER
                if (tMin >= 0.5 && tMax <= 50)
                {
                    // 🔥 PICK THE LARGEST VALID RANGE (PQR logic)
                    if (tMax > bestMax)
                    {
                        bestMin = tMin;
                        bestMax = tMax;
                    }
                }
            }

            if (bestMax > 0)
                return (bestMin, bestMax);

            // 🔥 FALLBACK → SINGLE VALUE (ONLY IF CLEAR)
            var singleMatches = Regex.Matches(text,
                @"THICKNESS[^0-9]{0,20}(\d+[\.,]?\d*)",
                RegexOptions.IgnoreCase);

            foreach (Match m in singleMatches)
            {
                var val = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);

                if (val >= 1 && val <= 50)
                {
                    return (1.5, val * 2);
                }
            }

            return (0, 0);
        }

        private (double min, double max)? ExtractFromDesignation(string text)
        {
            text = text.Replace(",", ".");


            // 🔥 FIX LINE BREAKS / SPLIT VALUES
            text = Regex.Replace(text, @"(\d+[\.,]?\d*)\s*-\s*(\r?\n|\s)+\s*(\d+[\.,]?\d*)", "$1-$3");

            // ONLY fix known OCR patterns like 1108 → 11.08
            text = Regex.Replace(text, @"\b(1[0-9]{3})\b", m =>
            {
                var val = m.Value;
                return val.Substring(0, 2) + "." + val.Substring(2);
            });

            var matches = Regex.Matches(text,
                @"THICKNESS[^0-9]{0,20}T?\s*(\d+[\.,]?\d*)\s*[-–]\s*(\d+[\.,]?\d*)",
                RegexOptions.IgnoreCase);

            var candidates = new List<(double min, double max)>();

            foreach (Match match in matches)
            {
                double min = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                double max = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                if (min >= 1 && max <= 50 && max > min)
                {
                    candidates.Add((min, max));
                }
            }

            if (candidates.Count == 0)
                return null;

            return candidates.OrderByDescending(c => c.max).First();
        }

        // =========================
        // 🔥 EXTRACT APPROVAL SECTION (CRITICAL FIX)
        // =========================
        private string ExtractApprovalSection(string text)
        {
            var match = Regex.Match(
                text,
                @"APPROVAL RANGE(.{0,5000})",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

            if (match.Success)
                return match.Value;

            return text;
        }

        private string ExtractDesignationSection(string text)
        {
            var match = Regex.Match(
                text,
                @"Designation(.{0,800})JOINT DESIGN",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (match.Success)
                return match.Value;

            return "";
        }


        private string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "WPS-UNKNOWN";

            return input.Replace(".pdf", "").Trim();
        }

        private string NormalizeText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Replace("§", "S")
                .Replace("—", "-")
                .Replace("_", "-");
        }

        private string NormalizeLine(
            string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return string.Empty;

            return line.Trim();
        }

        private string FindLine(
            IEnumerable<string> lines,
            params string[] keywords)
        {
            return lines.FirstOrDefault(line =>
                keywords.Any(keyword =>
                    line.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)))
                ?? string.Empty;
        }
    }
}
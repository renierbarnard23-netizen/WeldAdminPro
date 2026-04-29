using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using WeldAdminPro.Core.Quality;
using static System.Net.Mime.MediaTypeNames;

namespace WeldAdminPro.Data.Services
{
    public class WpsParserService
    {
        public List<Wps> ParseMultiple(string text, string fallbackName, Pqr? linkedPqr = null)
        {
            return new List<Wps> { ParseSingle(text, fallbackName, linkedPqr) };
        }

        private Wps ParseSingle(string text, string fallbackName, Pqr? linkedPqr = null)
        {
            text ??= "";

            text = text.ToUpper();

            System.IO.File.WriteAllText($@"C:\Temp\WPS_DEBUG_{DateTime.Now.Ticks}.txt", text);

            text = text.Replace("\n", " ").Replace("\r", " ");
            text = Regex.Replace(text, @"\b5A\b", "SA");

            var wps = new Wps
            {
                Id = Guid.NewGuid(),
                WpsNumber = Clean(fallbackName),
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

            Console.WriteLine($"{wps.WpsNumber} → {wps.PNumber}");

            // =========================
            // ✅ WPS NUMBER (FINAL FINAL FIX)
            // =========================

            string? raw = null;

            // 1️⃣ STRICT LABEL FIRST
            var strictMatch = Regex.Match(text,
                @"WPS\s*(Number|#|:)\s*([A-Z0-9\/\-\s]{5,})",
                RegexOptions.IgnoreCase);

            if (strictMatch.Success)
            {
                raw = strictMatch.Groups[2].Value;
            }
            else
            {
                // 2️⃣ CONTROLLED FALLBACK
                var fallbackMatch = Regex.Match(text,
                    @"\b(WPS[-\s]?[A-Z0-9\/\-]{3,})",
                    RegexOptions.IgnoreCase);

                if (fallbackMatch.Success)
                    raw = fallbackMatch.Value;
            }

            if (!string.IsNullOrWhiteSpace(raw))
            {
                // =========================
                // 🔥 HARD CLEAN FIRST (CRITICAL FIX)
                // =========================

                raw = raw.ToUpper();

                // Fix OCR errors FIRST
                raw = raw.Replace("5A", "SA");
                raw = raw.Replace("SA ", "SA");

                // Remove everything after REV / DATE
                raw = Regex.Split(raw, @"REV|DATE", RegexOptions.IgnoreCase)[0];

                // REMOVE ALL NON-WPS TRAILING TEXT EARLY
                raw = Regex.Replace(raw, @"(STEEL|PIPE|PLATE|S-STEEL).*", "", RegexOptions.IgnoreCase);

                raw = raw.Trim();

                // =========================
                // 🔥 SPECIAL FORMATS
                // =========================

                if (Regex.IsMatch(raw, @"DYN\/WPS\/[0-9\-]+"))
                {
                    raw = Regex.Match(raw, @"DYN\/WPS\/[0-9\-]+").Value;
                }
                else if (Regex.IsMatch(raw, @"DYNO1[-\s]?W"))
                {
                    raw = "DYNO1-W";
                }
                else if (Regex.IsMatch(raw, @"SB\s*163"))
                {
                    raw = "WPS-SB163";
                }
                else if (Regex.IsMatch(raw, @"WPS5418"))
                {
                    raw = "WPS5418-W1";
                }
                else if (Regex.IsMatch(raw, @"SAF\s*2507"))
                {
                    raw = "WPS-SAF2507";
                }
                else
                {
                    // =========================
                    // ✅ CLEAN NUMERIC EXTRACTION
                    // =========================

                    var match = Regex.Match(raw,
                        @"WPS\s*[-]?\s*(SA)?\s*([0-9]{3,4}[A-Z]?)");

                    if (match.Success)
                    {
                        var number = match.Groups[2].Value;
                        raw = $"WPS-SA{number}";
                    }
                    else
                    {
                        var fallback = Regex.Match(raw, @"WPS[-A-Z0-9\/]+");
                        if (fallback.Success)
                            raw = fallback.Value;
                    }
                }

                // =========================
                // FINAL CLEAN (SAFE NOW)
                // =========================

                raw = raw.Trim();
                raw = Regex.Replace(raw, @"[-_]+$", "");

                if (!string.IsNullOrWhiteSpace(raw) && raw.Length >= 6)
                    wps.WpsNumber = raw;
            }

            // FINAL SAFETY
            if (string.IsNullOrWhiteSpace(wps.WpsNumber))
            {
                wps.WpsNumber = $"{Clean(fallbackName)}-{DateTime.Now.Ticks}";
            }

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
            // 🔥 MATERIAL SECTION EXTRACTION (CRITICAL FIX)
            // =========================


            // Try isolate BASE MATERIAL section first
            string materialText;


            var materialSectionMatch = Regex.Match(text,
                @"(BASE MATERIAL|MATERIAL|PARENT MATERIAL).*?(FILLER|WELDING|PREHEAT|PROCESS|POSITION)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (materialSectionMatch.Success && materialSectionMatch.Value.Length > 50)
            {
                materialText = materialSectionMatch.Value.ToUpper();
            }
            else
            {
                // 🔥 JUST REUSE VARIABLE (NO var)
                materialSectionMatch = Regex.Match(text,
                    @"BASE METALS?.{0,800}",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (materialSectionMatch.Success)
                {
                    materialText = materialSectionMatch.Value.ToUpper();
                }
                else
                {
                    materialText = text.Substring(0, Math.Min(400, text.Length)).ToUpper();
                }
            }
            // 🔥 ONLY remove WPS references IF THEY ARE NOT MATERIAL LINES
            materialText = Regex.Replace(materialText,
                @"WPS[-\s]*SAF\s*2507\s*(REV|DATE)",
                "",
                RegexOptions.IgnoreCase);


            // OCR fixes
            materialText = materialText.Replace("5A", "SA");
            materialText = materialText.Replace("$", "S");

            // Normalize spacing
            materialText = Regex.Replace(materialText, @"[\-_/]", " ");
            materialText = Regex.Replace(materialText, @"\s+", " ");

            // 🔥 DIRECT SPEC EXTRACTION (MUST BE HERE)
            if (Regex.IsMatch(materialText, @"TP\s*304L|S30403"))
                wps.MaterialGroup = "Stainless 304L";

            else if (Regex.IsMatch(materialText, @"TP\s*310|S31000"))
                wps.MaterialGroup = "Stainless 310";

            else if (Regex.IsMatch(materialText, @"TP\s*316|S31600"))
                wps.MaterialGroup = "Stainless 316";

            // 🔥 ONLY detect SAF2507 if part of BASE MATERIAL SPEC (NOT WPS reference)
            if (Regex.IsMatch(materialText, @"A/SA[-\s]*790.*S32750", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(materialText, @"UNS\s*S32750", RegexOptions.IgnoreCase))
            {
                wps.MaterialGroup = "Duplex SAF2507";
            }

            // 🔥 FINAL FALLBACK — CHECK FULL TEXT (CRITICAL FIX)
            if (string.IsNullOrWhiteSpace(wps.MaterialGroup))
            {
                if (Regex.IsMatch(text, @"SAF\s*2507", RegexOptions.IgnoreCase))
                {
                    wps.MaterialGroup = "Duplex SAF2507";
                }
            }

            // =========================
            // 🔥 HARD PRIORITY MATERIAL OVERRIDE (NEW)
            // =========================

            // 🔥 PRIORITY MATERIAL (ORDER FIXED)

            if (string.IsNullOrWhiteSpace(wps.MaterialGroup) || wps.MaterialGroup == "UNKNOWN")
            {
                if (Regex.IsMatch(materialText, @"430A|BS\s*1501", RegexOptions.IgnoreCase))
                    wps.MaterialGroup = "Carbon Steel 430A";

                else if (Regex.IsMatch(materialText, @"S?A\s*106", RegexOptions.IgnoreCase))
                    wps.MaterialGroup = "Carbon Steel A106";

                else if (Regex.IsMatch(materialText, @"S?B\s*163"))
                    wps.MaterialGroup = "Nickel Alloy";

                else if (Regex.IsMatch(materialText, @"904L|N08904"))
                    wps.MaterialGroup = "Nickel Alloy 904L";

                else if (Regex.IsMatch(materialText, @"3\s*0\s*4\s*L"))
                    wps.MaterialGroup = "Stainless 304L";

                else if (Regex.IsMatch(materialText, @"\b316\b"))
                    wps.MaterialGroup = "Stainless 316";

                else if (Regex.IsMatch(materialText, @"\b310\b"))
                    wps.MaterialGroup = "Stainless 310";
            }         
            

            // =========================
            // 🔥 MATERIAL DETECTION (ORDER FIXED)
            // =========================

            System.IO.File.WriteAllText(@"C:\Temp\MATERIAL_DEBUG.txt", materialText);

            var scores = new Dictionary<string, int>();

            void AddScore(string key, int value)
            {
                if (!scores.ContainsKey(key))
                    scores[key] = 0;

                scores[key] += value;
            }                 

            // CARBON STEEL
            if (Regex.IsMatch(materialText, @"430A|BS\s*1501|A105|A106"))
                AddScore("Carbon Steel", 5);

            if (Regex.IsMatch(materialText, @"A106"))
                AddScore("Carbon Steel A106", 10);

            // STAINLESS
            // STAINLESS (FIXED - OCR SAFE)
            if (Regex.IsMatch(materialText, @"3\s*0\s*4\s*L"))
                AddScore("Stainless 304L", 20);

            if (Regex.IsMatch(materialText, @"\b3\s*1\s*6\s*L?\b"))
                AddScore("Stainless 316", 20);

            if (Regex.IsMatch(materialText, @"\b3\s*1\s*0\b"))
                AddScore("Stainless 310", 20);            

            // DUPLEX
            if (Regex.IsMatch(materialText, @"S32750") && Regex.IsMatch(materialText, @"A/SA\-790"))
            {
                AddScore("Duplex SAF2507", 20);
            }
            // NICKEL
            if (Regex.IsMatch(materialText, @"904L|N08904"))
            {
                AddScore("Nickel Alloy 904L", 15);
            }

            if (Regex.IsMatch(materialText, @"SB\s*163"))
            {
                AddScore("Nickel Alloy", 50); // 🔥 HIGH PRIORITY + UNIQUE
            }

            // TITANIUM
            if (Regex.IsMatch(materialText, @"R50400|GR\s*2|TITANIUM", RegexOptions.IgnoreCase))
                AddScore("Titanium", 15);
            // 🔥 STRONG PREFERENCE FOR ENGINEERING LIMITS


            // =========================
            // 🔥 FINAL MATERIAL LOCK (CRITICAL FIX)
            // =========================

            // 🚨 DO NOT TOUCH MATERIAL IF ALREADY SET
            if (!string.IsNullOrWhiteSpace(wps.MaterialGroup) && wps.MaterialGroup != "UNKNOWN")
            {
                // LOCK IT — DO NOTHING
            }
            else if (scores.Count > 0)
            {
                var best = scores.OrderByDescending(x => x.Value).First().Key;

                wps.MaterialGroup = best switch
                {
                    "Carbon Steel A106" => "Carbon Steel A106",
                    "Carbon Steel" => "Carbon Steel 430A",
                    _ => best
                };
            }


            // =========================
            // 🔥 FINAL THICKNESS LOGIC (CLEAN & STABLE)
            // =========================

            if (linkedPqr != null)
            {
                // ✅ ALWAYS TRUST PQR
                wps.ThicknessMin = linkedPqr.ThicknessQualifiedMin;
                wps.ThicknessMax = linkedPqr.ThicknessQualifiedMax;
            }
            else
            {
                // ✅ PRIORITY 1: DESIGNATION (MOST RELIABLE)
                var designation = ExtractFromDesignation(text);

                if (designation.HasValue)
                {
                    wps.ThicknessMin = designation.Value.min;
                    wps.ThicknessMax = designation.Value.max;
                }
                else
                {
                    // ✅ PRIORITY 2: APPROVAL RANGE
                    var approvalText = ExtractApprovalSection(text);
                    var approval = ExtractFromApproval(approvalText);

                    if (approval.HasValue)
                    {
                        wps.ThicknessMin = approval.Value.min;
                        wps.ThicknessMax = approval.Value.max;
                    }
                    else
                    {
                        // ✅ PRIORITY 3: TEST THICKNESS
                        var result = ExtractTestThickness(text);
                        wps.ThicknessMin = result.min;
                        wps.ThicknessMax = result.max;
                    }
                }
            }


            // =========================
            // ✅ P FROM MATERIAL
            // =========================

            // 🔥 FORCE OVERRIDE FROM MATERIAL (FINAL FIX)
            var mat = (wps.MaterialGroup ?? "").ToUpper();

            if (mat.Contains("304") || mat.Contains("310") || mat.Contains("316"))
                wps.PNumber = "P8";

            else if (mat.Contains("A106") || mat.Contains("430"))
                wps.PNumber = "P1";

            else if (mat.Contains("2507"))
                wps.PNumber = "P10H";

            // 🔥 SB163 MUST BE P41 (CRITICAL FIX)
            else if (mat.Contains("SB163"))
                wps.PNumber = "P41";

            // 904L → P45
            else if (mat.Contains("904"))
                wps.PNumber = "P45";

            // Other nickel alloys → default P45 (safe fallback)
            else if (mat.Contains("NICKEL"))
                wps.PNumber = "P45";

            else if (mat.Contains("TITANIUM") || mat.Contains("R50400"))
                wps.PNumber = "P51";

            // =========================
            // 🔥 EXTRACT FILLER SECTION FIRST (NEW)
            // =========================

            string fillerText;

            var fillerSection = Regex.Match(text,
                   @"FILLER\s*METALS?.{0,2000}",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (fillerSection.Success && fillerSection.Value.Length > 50)
            {
                fillerText = fillerSection.Value;
            }
            else
            {
                fillerText = text;
            }

            System.IO.File.WriteAllText(@"C:\Temp\FILLER_SECTION.txt", fillerText);

            // =========================
            // ✅ F NUMBER (FIXED)
            // =========================

            var fillermatches = Regex.Matches(fillerText,
                @"\bE\s*R\s*\d\s*\d\s*\d\s*[A-Z0-9\-]*\b" +   // OCR broken ER (E R 3 1 6 L)
                @"|\bER\s*\d{2,4}[A-Z0-9\-]*\b" +            // Normal ER308L
                @"|\bE\s*7\s*0\s*1\s*8\b" +                  // E7018
                @"|\bE\s*R\s*T\s*I[-\s]?\d+\b" +             // ERTI broken
                @"|\bERTI[-\s]?\d+\b" +                     // ERTI-1
                @"|\bE\s*R\s*N\s*I[A-Z0-9\-]*\b" +           // ERNi broken
                @"|\bERNI[A-Z0-9\-]*\b" +                   // ERNiCrMo
                @"|\bINCONEL\b",
            RegexOptions.IgnoreCase);

            string bestFiller = "";

            foreach (Match m in fillermatches)
            {
                var val = Regex.Replace(m.Value.ToUpper(), @"\s+", "");

                if (val.Contains("ERTI"))
                {
                    bestFiller = val;
                    wps.FNumber = "F51";
                    break;
                }

                if (val.Contains("ERNI") || val.Contains("NICR") || val.Contains("INCONEL"))
                {
                    bestFiller = val;
                    wps.FNumber = "F41";
                    break;
                }

                if (val.Contains("308") || val.Contains("316") || val.Contains("2594"))
                {
                    bestFiller = val;
                    wps.FNumber = "F6";
                    break;
                }

                if (string.IsNullOrWhiteSpace(wps.FNumber))
                {
                    if (Regex.IsMatch(text, @"\b41\s*-\s*41\b"))
                        wps.FNumber = "F41";
                }

                // 🔥 DO NOT assign F4 here at all
            }

            // =========================
            // 🔥 INTELLIGENT FALLBACK (CRITICAL FINAL FIX)
            // =========================

            if (string.IsNullOrWhiteSpace(wps.FNumber))
            {
                var mat2 = (wps.MaterialGroup ?? "").ToUpper();

                if (mat2.Contains("304") || mat2.Contains("316") || mat2.Contains("310"))
                    wps.FNumber = "F6";

                else if (mat2.Contains("CARBON") || mat2.Contains("A106") || mat2.Contains("430"))
                    wps.FNumber = "F6";

                else if (mat2.Contains("904"))
                    wps.FNumber = "F6";   // ✅ MOST COMMON FOR 904L

                else if (mat2.Contains("NICKEL"))
                    wps.FNumber = "F41";  // ✅ GENERIC NICKEL ALLOYS
                if (mat.Contains("SB163"))
                {
                    wps.FNumber = "F41";
                }
                else if (mat2.Contains("TITANIUM"))
                {
                    wps.FNumber = "F51";
                }

                else if (mat2.Contains("2507"))
                    wps.FNumber = "F6";
            }

            System.IO.File.WriteAllText(@"C:\Temp\FILLER_FOUND.txt", bestFiller);


            // 🔥 FALLBACK (KEEP THIS)
            if (string.IsNullOrWhiteSpace(wps.FNumber))
            {
                var global = text.ToUpper();

                if (Regex.IsMatch(global, @"ER308|ER316|ER2594"))
                    wps.FNumber = "F6";

                else if (Regex.IsMatch(global, @"ERTI"))
                    wps.FNumber = "F51";

                else if (Regex.IsMatch(global, @"ERNI|NICR|INCONEL"))
                    wps.FNumber = "F41";

                else if (Regex.IsMatch(global, @"E7018|ER70S6|ER70"))
                    wps.FNumber = "F4";
            }

            System.IO.File.WriteAllText(@"C:\Temp\FILLER_DEBUG.txt", text);

            // =========================
            // ✅ POSITION
            // =========================

            if (Regex.IsMatch(text, @"6G|ANY", RegexOptions.IgnoreCase))
                wps.Position = "ALL";
            else if (Regex.IsMatch(text, @"4G"))
                wps.Position = "O;F";

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

            return wps;
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
            var match = Regex.Match(text,
                @"APPROVAL RANGE(.{0,2000})WELDING DATA",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (match.Success)
                return match.Value;

            // 🚨 DO NOT RETURN FULL TEXT
            return "";
        }


        private string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "WPS-UNKNOWN";

            return input.Replace(".pdf", "").Trim();
        }
    }
}
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Services
{
    public class QualificationRangeEngine
    {
        public QualificationRangeResult Calculate(Pqr pqr)
        {
            var result = new QualificationRangeResult();

            var t = pqr.ThicknessTested;

            if (t <= 0)
                return result;

            // =========================
            // THICKNESS (QW-451 SIMPLIFIED)
            // =========================

            if (t < 1.5)
            {
                result.MinThickness = t;
                result.MaxThickness = 2 * t;
                result.Notes = "Thin material rule";
            }
            else if (t >= 1.5 && t <= 10)
            {
                result.MinThickness = 1.5;
                result.MaxThickness = 2 * t;
                result.Notes = "Standard plate qualification";
            }
            else // t > 10
            {
                result.MinThickness = 5;
                result.MaxThickness = double.MaxValue;
                result.Notes = "Unlimited thickness qualification";
            }



            // =========================
            // DIAMETER (PIPE)
            // =========================

            if (pqr.DiameterMax > 0)
            {
                var d = pqr.DiameterMax;

                if (d < 25)
                {
                    result.MinDiameter = d;
                    result.MaxDiameter = 2 * d;
                }
                else
                {
                    result.MinDiameter = 0;
                    result.MaxDiameter = double.MaxValue;
                }
            }

            // =========================
            // POSITION EXPANSION
            // =========================

            result.QualifiedPosition = ExpandPosition(pqr.QualifiedPosition);

            return result;
        }

        private string ExpandPosition(string pos)
        {
            pos = pos?.ToUpper() ?? "";

            return pos switch
            {
                "6G" => "ALL",
                "5G" => "1G,2G,5G",
                "3G" => "1G,2G,3G",
                _ => pos
            };
        }

        public List<EssentialVariableResult> Evaluate(Wps wps, Pqr pqr)
        {
            var results = new List<EssentialVariableResult>();

            if (wps == null || pqr == null)
                return results;

            // =========================
            // THICKNESS CHECK
            // =========================
            if (wps.ThicknessMin < pqr.ThicknessQualifiedMin ||
                wps.ThicknessMax > pqr.ThicknessQualifiedMax)
            {
                results.Add(new EssentialVariableResult
                {
                    Code = "QW-451",
                    Variable = "Thickness",
                    Message = $"WPS thickness {wps.ThicknessMin}-{wps.ThicknessMax} outside qualified range {pqr.ThicknessQualifiedMin}-{pqr.ThicknessQualifiedMax}",
                    IsFailure = true
                });
            }

            // =========================
            // DIAMETER CHECK (PIPE)
            // =========================
            if (wps.Diameter > 0 && pqr.DiameterMax > 0)
            {
                if (wps.Diameter < pqr.DiameterMin ||
                    wps.Diameter > pqr.DiameterMax)
                {
                    results.Add(new EssentialVariableResult
                    {
                        Code = "QW-452",
                        Variable = "Diameter",
                        Message = $"Diameter {wps.Diameter} outside qualified range {pqr.DiameterMin}-{pqr.DiameterMax}",
                        IsFailure = true
                    });
                }
            }

            // =========================
            // POSITION RANGE (already partly handled elsewhere, but safe here)
            // =========================
            if (!IsPositionQualified(wps.Position, pqr.QualifiedPosition))
            {
                results.Add(new EssentialVariableResult
                {
                    Code = "QW-405",
                    Variable = "Position",
                    Message = $"{wps.Position} not qualified by {pqr.QualifiedPosition}",
                    IsFailure = true
                });
            }

            return results;
        }

        private bool IsPositionQualified(string? wpsPos, string? pqrPos)
        {
            if (string.IsNullOrWhiteSpace(wpsPos) || string.IsNullOrWhiteSpace(pqrPos))
                return true;

            wpsPos = wpsPos.ToUpper();
            pqrPos = pqrPos.ToUpper();

            if (pqrPos == "6G") return true;

            if (pqrPos == "5G")
                return wpsPos is "1G" or "2G" or "5G";

            if (pqrPos == "3G")
                return wpsPos is "1G" or "2G" or "3G";

            return wpsPos == pqrPos;
        }
    }
}
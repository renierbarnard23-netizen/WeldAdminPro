using WeldAdminPro.Core.Quality;

    namespace WeldAdminPro.Core.Services
    {
        public class QualificationRangeEngine
        {
            public QualificationRangeResult Calculate(Pqr pqr)
            {
                var result = new QualificationRangeResult();

                if (pqr == null)
                    return result;

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
            // DIAMETER
            // =========================

            // =========================
            // DIAMETER
            // =========================
            //
            // The PQR records the actual test coupon diameter.
            // Diameter qualification restrictions are not applied
            // at the procedure qualification range level.
            //
            // The actual tested diameter remains available through
            // pqr.DiameterTested.
            //
            // Welder-performance diameter restrictions belong in
            // the welder qualification/performance validation layer.
            // =========================

            result.MinDiameter = 0;
            result.MaxDiameter = double.MaxValue;

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
        }
    }


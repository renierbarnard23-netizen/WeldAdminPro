using System;

namespace WeldAdminPro.Core.Services
{
    public class DiameterQualificationService
    {
        public bool IsQualified(
    double wpsDiameter,
    double qualifiedMin,
    double qualifiedMax)
        {
            // Plate qualification
            if (qualifiedMin <= 0 &&
                qualifiedMax <= 0)
            {
                return true;
            }

            // Unlimited qualification
            if (wpsDiameter <= 0)
                return true;

            if (qualifiedMax == double.MaxValue)
                return true;

            return
                wpsDiameter >= qualifiedMin
                &&
                wpsDiameter <= qualifiedMax;
        }

        public DiameterQualificationResult Evaluate(
            double couponDiameter)
        {
            var result =
                new DiameterQualificationResult();

            // =====================================
            // INVALID INPUT
            // =====================================

            if (couponDiameter <= 0)
            {
                result.IsValid = false;

                result.Reason =
                    "Invalid coupon diameter.";

                return result;
            }

            // =====================================
            // SMALL BORE
            // =====================================

            if (couponDiameter < 25)
            {
                result.MinDiameter = couponDiameter;

                result.MaxDiameter = 2 * couponDiameter;
            }

            // =====================================
            // STANDARD PIPE
            // =====================================

            else if (couponDiameter < 73)
            {
                result.MinDiameter = 25;

                result.MaxDiameter = double.MaxValue;
            }

            // =====================================
            // LARGE PIPE
            // =====================================

            else
            {
                result.MinDiameter = double.MaxValue;

                result.MaxDiameter = double.MaxValue;
            }

            result.IsValid = true;

            return result;
        }
    }

    public class DiameterQualificationResult
    {
        public bool IsValid { get; set; }

        public double MinDiameter { get; set; }

        public double MaxDiameter { get; set; }

        public string Reason { get; set; } = "";
    }
}
using System;

namespace WeldAdminPro.Core.Services
{
    public class ThicknessQualificationService
    {
        public ThicknessQualificationResult Evaluate(
            double couponThickness,
            bool hasPwht)
        {
            var result =
                new ThicknessQualificationResult();

            // =====================================
            // INVALID INPUT
            // =====================================

            if (couponThickness <= 0)
            {
                result.IsValid = false;

                result.Reason =
                    "Invalid coupon thickness.";

                return result;
            }

            // =====================================
            // ASME IX QW-451
            // =====================================

            // T < 1.5 mm
            if (couponThickness < 1.5)
            {
                result.MinThickness =
                    couponThickness;

                result.MaxThickness =
                    couponThickness * 2;
            }

            // 1.5 mm to < 19 mm
            else if (couponThickness < 19)
            {
                result.MinThickness =
                    1.5;

                result.MaxThickness =
                    couponThickness * 2;
            }

            // >= 19 mm
            else
            {
                result.MinThickness =
                    1.5;

                result.MaxThickness =
                    double.MaxValue;
            }

            // =====================================
            // PWHT EFFECTS (future expansion)
            // =====================================

            if (hasPwht)
            {
                result.Notes =
                    "PWHT qualification applied.";
            }

            result.IsValid = true;

            return result;
        }

        public bool IsQualified(
            double wpsMin,
            double wpsMax,
            double qualifiedMin,
            double qualifiedMax)
        {
            // Unlimited thickness support
            var maxQualified =
                qualifiedMax == double.MaxValue
                    ? double.MaxValue
                    : qualifiedMax;

            return
                wpsMin >= qualifiedMin
                &&
                wpsMax <= maxQualified;
        }
    }

    public class ThicknessQualificationResult
    {
        public bool IsValid { get; set; }

        public double MinThickness { get; set; }

        public double MaxThickness { get; set; }

        public string Notes { get; set; } = "";

        public string Reason { get; set; } = "";
    }
}
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
    }
}
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WpsGeneratorService
    {
        public Wps GenerateFromPqr(Pqr pqr)
        {
            if (pqr == null)
                throw new ArgumentNullException(nameof(pqr));

            var wps = new Wps
            {
                // =====================================================
                // ID / PQR LINK
                // =====================================================

                Id = Guid.NewGuid(),
                PqrId = pqr.Id,
                LinkedPqrNumber = pqr.PqrNumber,

                // =====================================================
                // IDENTIFICATION
                // =====================================================

                WpsNumber = $"WPS-{pqr.PqrNumber}-01",
                Process = pqr.Process,

                // =====================================================
                // MATERIAL
                // =====================================================

                MaterialGroup = pqr.MaterialGroup,
                PNumber = pqr.PNumber,

                // =====================================================
                // FILLER / GAS
                // =====================================================

                FillerMaterial = pqr.FillerMaterial ?? "",
                GasType = pqr.GasType ?? "",

                // =====================================================
                // F-NUMBER
                // =====================================================

                FNumber = pqr.FNumber,

                // =====================================================
                // POSITION
                // =====================================================

                Position = GetQualifiedPosition(pqr.QualifiedPosition),

                // =====================================================
                // JOINT
                // =====================================================

                JointType = pqr.JointType,
                JointDesign = pqr.JointDesign,

                // =====================================================
                // BACKING
                // =====================================================

                Backing = pqr.Backing,
                BackingType = pqr.BackingType,

                // =====================================================
                // THICKNESS - ASME IX QUALIFICATION RANGE
                // =====================================================

                ThicknessMin =
                    CalculateMinThickness(pqr.ThicknessTested),

                ThicknessMax =
                    CalculateMaxThickness(pqr.ThicknessTested),

                QualifiedThicknessRange =
                    $"{CalculateMinThickness(pqr.ThicknessTested):0.##} - " +
                    $"{CalculateMaxThickness(pqr.ThicknessTested):0.##}",

                // =====================================================
                // DIAMETER
                // =====================================================
                //
                // Do NOT use double.MaxValue when the PQR contains
                // no diameter information. That creates the
                // 1.79769313486231... value seen in the WPS editor.
                //
                // If a qualified maximum exists, use it.
                // Otherwise use the qualified minimum when available.
                // Otherwise leave the WPS diameter at 0.
                //

                // Use the actual PQR test coupon diameter.
                // Do not display double.MaxValue as a physical diameter.
                Diameter = pqr.DiameterTested > 0
                    ? pqr.DiameterTested
                    : CalculateDiameter(
                        pqr.DiameterMin,
                        pqr.DiameterMax),

                // =====================================================
                // ELECTRICAL VARIABLES
                // =====================================================

                AmpsMin = pqr.AmpsUsed,
                AmpsMax = pqr.AmpsUsed,

                VoltsMin = pqr.VoltsUsed,
                VoltsMax = pqr.VoltsUsed,

                HeatInputMin = pqr.HeatInput,
                HeatInputMax = pqr.HeatInput,

                // =====================================================
                // PREHEAT / INTERPASS
                // =====================================================

                PreheatMin = pqr.Preheat,
                PreheatMax = pqr.Preheat,

                InterpassMax =
                    pqr.Interpass > 0
                        ? pqr.Interpass
                        : null,

                // =====================================================
                // PWHT
                // =====================================================

                PwhtRequired = pqr.PwhtPerformed,

                // =====================================================
                // ESSENTIAL VARIABLES
                // =====================================================

                CurrentType = pqr.CurrentType,
                Progression = pqr.Progression,

                // =====================================================
                // NEW WPS DOCUMENT CONTROL STATE
                // =====================================================

                Revision = 0,
                IsActive = true,
                IsApproved = false,
                IsLocked = false,

                // =====================================================
                // COMPLIANCE
                // =====================================================

                IsCompliant = false,
                ValidationStatus = ""
            };

            return wps;
        }

        // =============================================================
        // ASME IX THICKNESS QUALIFICATION
        // =============================================================

        private double CalculateMinThickness(double testThickness)
        {
            if (testThickness <= 0)
                return 0;

            if (testThickness <= 12)
                return testThickness / 2;

            return 5;
        }

        private double CalculateMaxThickness(double testThickness)
        {
            if (testThickness <= 0)
                return 0;

            if (testThickness <= 12)
                return testThickness * 2;

            return double.MaxValue;
        }

        // =============================================================
        // DIAMETER
        // =============================================================

        private double CalculateDiameter(
            double diameterMin,
            double diameterMax)
        {
            if (diameterMax > 0)
                return diameterMax;

            if (diameterMin > 0)
                return diameterMin;

            return 0;
        }

        // =============================================================
        // POSITION
        // =============================================================

        private string GetQualifiedPosition(string? pqrPosition)
        {
            if (string.IsNullOrWhiteSpace(pqrPosition))
                return "ALL";

            pqrPosition = pqrPosition.Trim().ToUpperInvariant();

            if (pqrPosition == "6G")
                return "ALL";

            if (pqrPosition == "5G")
                return "1G,2G,5G";

            return pqrPosition;
        }
    }
}


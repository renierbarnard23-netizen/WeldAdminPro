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
                Id = Guid.NewGuid(),

                // 🔗 Link
                PqrId = pqr.Id,
                WpsNumber = $"WPS-{pqr.PqrNumber}-01",

                // =========================
                // COPY DIRECT
                // =========================
                Process = pqr.Process,
                PNumber = pqr.PNumber,
                FNumber = pqr.FNumber,
                JointType = pqr.JointType,

                // =========================
                // THICKNESS (ASME IX QW-451)
                // =========================
                ThicknessMin = CalculateMinThickness(pqr.ThicknessTested),
                ThicknessMax = CalculateMaxThickness(pqr.ThicknessTested),

                // =========================
                // DIAMETER (PIPE RULES)
                // =========================
                Diameter = CalculateDiameter(pqr.DiameterMax),

                // =========================
                // POSITION
                // =========================
                Position = GetQualifiedPosition(pqr.QualifiedPosition),
             
            };

            return wps;
        }

        // =========================
        // ASME IX LOGIC
        // =========================

        private double CalculateMinThickness(double testThickness)
        {
            if (testThickness <= 12)
                return testThickness / 2;

            return 5; // ASME lower limit
        }

        private double CalculateMaxThickness(double testThickness)
        {
            if (testThickness <= 12)
                return testThickness * 2;

            return double.MaxValue; // No upper limit
        }

        private double CalculateDiameter(double max)
        {
            if (max <= 0)
                return double.MaxValue;

            return max;
        }

        private string GetQualifiedPosition(string? pqrPosition)
        {
            if (string.IsNullOrWhiteSpace(pqrPosition))
                return "ALL";

            pqrPosition = pqrPosition.ToUpper();

            if (pqrPosition == "6G")
                return "ALL";

            if (pqrPosition == "5G")
                return "1G,2G,5G";

            return pqrPosition;
        }
    }
}
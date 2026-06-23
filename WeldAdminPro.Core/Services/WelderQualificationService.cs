using System;

namespace WeldAdminPro.Core.Services
{
    public class WelderQualificationService
    {
        private readonly PositionQualificationService
            _positionService =
                new();

        private readonly MaterialQualificationService
            _materialService =
                new();

        public WelderQualificationResult Evaluate(
            string welderNumber,
            string wpqrProcess,
            string wpqrPosition,
            string wpqrPNumber,
            DateTime qualificationDate,

            string weldProcess,
            string weldPosition,
            string weldPNumber)
        {
            var result =
                new WelderQualificationResult();

            // =====================================
            // BASIC CHECKS
            // =====================================

            if (string.IsNullOrWhiteSpace(
                welderNumber))
            {
                result.IsQualified = false;

                result.Reason =
                    "Welder number missing.";

                return result;
            }

            // =====================================
            // PROCESS
            // =====================================

            if (!string.Equals(
                wpqrProcess,
                weldProcess,
                StringComparison.OrdinalIgnoreCase))
            {
                result.IsQualified = false;

                result.Reason =
                    $"Process {weldProcess} not qualified.";

                return result;
            }

            // =====================================
            // POSITION
            // =====================================

            var positionQualified =
                _positionService.IsQualified(
                    weldPosition,
                    wpqrPosition);

            if (!positionQualified)
            {
                result.IsQualified = false;

                result.Reason =
                    $"Position {weldPosition} not qualified.";

                return result;
            }

            // =====================================
            // MATERIAL
            // =====================================

            System.Diagnostics.Debug.WriteLine(
    $"WPQR Material = [{wpqrPNumber}]");

            System.Diagnostics.Debug.WriteLine(
                $"Weld Material = [{weldPNumber}]");

            var materialQualified =
                _materialService.IsQualified(
                    weldPNumber,
                    wpqrPNumber);

            if (!materialQualified)
            {
                result.IsQualified = false;

                result.Reason =
                    $"Material {weldPNumber} not qualified.";

                return result;
            }

            // =====================================
            // EXPIRY CHECK
            // =====================================

            var days =
                (DateTime.Now - qualificationDate)
                .TotalDays;

            if (days > 180)
            {
                result.IsQualified = false;

                result.Reason =
                    "Welder qualification expired.";

                return result;
            }

            // =====================================
            // PASS
            // =====================================

            result.IsQualified = true;

            result.Reason =
                "Welder qualified.";

            return result;
        }
    }

    public class WelderQualificationResult
    {
        public bool IsQualified { get; set; }

        public string Reason { get; set; } = "";
    }
}
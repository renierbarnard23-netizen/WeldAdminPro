namespace WeldAdminPro.Core.Quality.Normalization
{
    public static class ComplianceNormalizer
    {
        // =========================
        // PROCESS
        // =========================

        public static string NormalizeProcess(
            string process)
        {
            if (string.IsNullOrWhiteSpace(process))
                return "";

            process = process
                .Trim()
                .ToUpper();

            if (process.Contains("TIG")
                || process.Contains("GTAW"))
            {
                return "GTAW";
            }

            if (process.Contains("MIG")
                || process.Contains("GMAW"))
            {
                return "GMAW";
            }

            if (process.Contains("STICK")
                || process.Contains("SMAW"))
            {
                return "SMAW";
            }

            return process;
        }

        // =========================
        // MATERIAL GROUP
        // =========================

        public static string NormalizeMaterialGroup(
    string material)
        {
            if (string.IsNullOrWhiteSpace(material))
                return "";

            material = material.Trim().ToUpper();

            // P8 Stainless
            if (material.Contains("304") ||
                material.Contains("316") ||
                material.Contains("P8") ||
                material == "8" ||
                material.Contains("SS") ||
                material.Contains("STAINLESS"))
            {
                return "P8";
            }

            // P1 Carbon steels
            if (material.Contains("P1") ||
                material == "1" ||
                material.Contains("CS") ||
                material.Contains("CARBON") ||
                material.Contains("A106") ||
                material.Contains("A516") ||
                material.Contains("GR70") ||
                material.Contains("SA106") ||
                material.Contains("SA516"))
            {
                return "P1";
            }

            return material;
        }

        // =========================
        // POSITION
        // =========================

        public static string NormalizePosition(
            string position)
        {
            if (string.IsNullOrWhiteSpace(position))
                return "";

            position = position
                .Trim()
                .ToUpper();

            // ISO / ASME equivalents
            if (position == "H-L045")
                return "6G";

            return position;
        }
    }
}
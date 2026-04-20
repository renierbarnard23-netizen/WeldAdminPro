namespace WeldAdminPro.Core.Quality.Services
{
    public class ValidationService
    {
        public (bool IsValid, string Message) Validate(Wps wps, WelderQualification welder)
        {
            // 1. PQR linked
            if (wps.PqrId == null)
                return (false, "WPS has no PQR assigned");

            // 2. Process
            if (!string.Equals(wps.Process, welder.Process, StringComparison.OrdinalIgnoreCase))
                return (false, "Welder not qualified for process");

            // 3. Expiry
            if (welder.ExpiryDate < DateTime.Today)
                return (false, "Welder qualification expired");

            // 4. P-No match
            if (wps.PNumber != welder.PNumber)
                return (false, $"P-No mismatch (WPS: {wps.PNumber}, Welder: {welder.PNumber})");

            // 5. F-No match
            if (wps.FNumber != welder.FNumber)
                return (false, $"F-No mismatch (WPS: {wps.FNumber}, Welder: {welder.FNumber})");

            // 6. Thickness range
            if (welder.MinThickness > wps.ThicknessMin || welder.MaxThickness < wps.ThicknessMax)
                return (false, "Thickness out of qualified range");

            // 7. Diameter
            if (wps.Diameter != double.MaxValue && welder.Diameter < wps.Diameter)
                return (false, "Welder not qualified for diameter");

            return (true, "Valid");
        }
    }
}
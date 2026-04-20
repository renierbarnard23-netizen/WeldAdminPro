using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
    public class WelderWpsValidationService
    {
        public (bool IsValid, List<string> Errors) Validate(Wps wps, WelderQualification wpqr)
        {
            var errors = new List<string>();

            if (wpqr == null)
            {
                errors.Add("No welder qualification found");
                return (false, errors);
            }

            if (wpqr.ExpiryDate < DateTime.Today)
                errors.Add("Welder qualification expired");

            if (wps.Process != wpqr.Process)
                errors.Add("Process not qualified");

            if (wps.MaterialGroup != wpqr.MaterialGroup)
                errors.Add("Material group not qualified");

            if (wps.Position != wpqr.Position)
                errors.Add("Position not qualified");

            if (wps.ThicknessMin < wpqr.ThicknessMin || wps.ThicknessMax > wpqr.ThicknessMax)
            {
                errors.Add("WPS thickness range not covered by welder qualification");
            }

            return (!errors.Any(), errors);
        }
    }
}
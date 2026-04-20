using QualityWps = WeldAdminPro.Core.Quality.Wps;
using QualityPqr = WeldAdminPro.Core.Quality.Pqr;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class WpsStatusService
    {
        private readonly WpsRepository _wpsRepo = new();
        private readonly PqrRepository _pqrRepo = new();
        private readonly WpsValidationService _validator = new();

        public List<(QualityWps wps, bool isValid, string message)> GetAllWithStatus()
        {
            var result = new List<(QualityWps, bool, string)>();

            var wpsList = _wpsRepo.GetAll();
            var pqrs = _pqrRepo.GetAll();

            foreach (var wps in wpsList)
            {
                var pqr = wps.PqrId == null
                    ? null
                    : pqrs.FirstOrDefault(p => p.Id == wps.PqrId);

                if (pqr == null)
                {
                    result.Add((wps, false, "No PQR linked"));
                    continue;
                }

                var validation = _validator.Validate(wps, pqr);

                result.Add((wps, validation.IsValid, validation.Message));
            }

            return result;
        }
    }
}
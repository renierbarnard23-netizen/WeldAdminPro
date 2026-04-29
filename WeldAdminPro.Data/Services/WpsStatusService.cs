using System.Linq;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Services;
using QualityWps = WeldAdminPro.Core.Quality.Wps;
using QualityPqr = WeldAdminPro.Core.Quality.Pqr;

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

                var errors = _validator.Validate(wps, pqr);

                if (errors.Any())
                {
                    result.Add((wps, false, string.Join("; ", errors)));
                }
                else
                {
                    result.Add((wps, true, "WPS COMPLIANT"));
                }
            }

            return result;
        }
    }
}
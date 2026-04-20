using System.Linq;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class ProjectComplianceService
    {
        private readonly WpsRepository _wpsRepo = new();
        private readonly PqrRepository _pqrRepo = new();
        private readonly ProjectDocumentRepository _docRepo = new();

        public ProjectComplianceResult Evaluate(Guid projectId)
        {
            var result = new ProjectComplianceResult();

            var validator = new WpsValidationService();

            var wpsList = _wpsRepo.GetAll();
            var pqrs = _pqrRepo.GetAll();

            foreach (var wps in wpsList)
            {
                var pqr = pqrs.FirstOrDefault(p => p.Id == wps.PqrId);

                if (pqr == null)
                {
                    result.WpsInvalid++;
                    result.Issues.Add($"WPS {wps.WpsNumber}: No PQR linked");
                    continue;
                }

                var (isValid, message) = validator.Validate(wps, pqr);

                if (!isValid)
                {
                    result.WpsInvalid++;
                    result.Issues.Add($"WPS {wps.WpsNumber}: {message}");
                }
                else
                {
                    result.WpsValid++;
                }
            }

            // 🔹 Welders
            var welderService = new WelderValidationService();
            var (expired, _) = welderService.Check();

            result.WeldersExpired = expired;

            if (expired > 0)
                result.Issues.Add($"{expired} welders expired");

            // 🔹 Documents
            var docs = _docRepo.GetByProject(projectId);

            var required = docs.Where(d => d.IsRequired).ToList();

            if (required.Any())
            {
                var uploaded = required.Count(d => d.IsUploaded);
                result.DocumentCompliancePercent = (int)((double)uploaded / required.Count * 100);
            }
            else
            {
                result.DocumentCompliancePercent = 100;
            }

            if (result.DocumentCompliancePercent < 100)
                result.Issues.Add("Missing required documents");

            // 🔥 FINAL DECISION
            result.IsCompliant =
                result.WpsInvalid == 0 &&
                result.WeldersExpired == 0 &&
                result.DocumentCompliancePercent == 100;

            return result;
        }
    }
}
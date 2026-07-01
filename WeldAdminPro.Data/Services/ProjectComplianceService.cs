using System;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
    public class ProjectComplianceService
    {
        private readonly WpsRepository _wpsRepo = new();
        private readonly PqrRepository _pqrRepo = new();
        private readonly ProjectDocumentRepository _docRepo = new();
        private readonly WpsComplianceService _wpsCompliance = new();

        public ProjectComplianceResult Evaluate(Guid projectId)
        {
            var result = new ProjectComplianceResult();

            // ⚠️ TEMP: No ProjectId link yet → use ALL WPS
            var wpsList = _wpsRepo.GetAll();

            result.TotalWps = wpsList.Count;

            foreach (var wps in wpsList)
            {
                if (wps.PqrId == null)
                {
                    result.Issues.Add($"WPS {wps.WpsNumber}: No PQR linked");
                    continue;
                }

                var pqr = _pqrRepo.GetById(wps.PqrId.Value);

                if (pqr == null)
                {
                    result.Issues.Add($"WPS {wps.WpsNumber}: Invalid PQR reference");
                    continue;
                }

                var compliance = _wpsCompliance.Evaluate(wps, pqr);

                if (compliance.IsCompliant)
                {
                    result.CompliantWps++;
                }
                else
                {
                    result.Issues.Add($"WPS {wps.WpsNumber}: Not compliant");
                }
            }

            // =========================
            // WELDERS
            // =========================
            var welderService = new WelderValidationService();
            var (expired, _) = welderService.Check();

            if (expired > 0)
            {
                result.Issues.Add($"{expired} welders expired");
            }


            // =========================
            // DOCUMENTS (ISO 3834 CORE)
            // =========================
            var docs = _docRepo.GetByProject(projectId);

            var requiredDocs = docs.Where(d => d.IsRequired).ToList();

            if (requiredDocs.Any())
            {
                var uploaded = requiredDocs.Count(d => d.IsUploaded);
                var approved = requiredDocs.Count(d => d.IsApproved);

                result.DocumentCompliancePercent =
                    (double)approved / requiredDocs.Count * 100;

                foreach (var doc in requiredDocs)
                {
                    if (!doc.IsUploaded)
                        result.Issues.Add($"Missing document: {doc.DocumentType}");

                    if (doc.IsUploaded && !doc.IsApproved)
                        result.Issues.Add($"Not approved: {doc.DocumentType}");
                }
            }
            else
            {
                result.DocumentCompliancePercent = 100;
            }

            return result;
        }
    }
}
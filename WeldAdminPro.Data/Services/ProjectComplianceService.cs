using System;
using System.Linq;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
    public class ProjectComplianceService
    {
        private readonly WpsRepository _wpsRepo = new();
        private readonly PqrRepository _pqrRepo = new();
        private readonly WelderQualificationRepository _welderQualificationRepo = new();
        private readonly ProjectDocumentRepository _docRepo = new();
        private readonly WpsComplianceService _wpsCompliance = new();

        public ProjectComplianceResult Evaluate(Guid projectId, Guid companyId)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException(
                    "Project Id is required.",
                    nameof(projectId));

            if (companyId == Guid.Empty)
                throw new ArgumentException(
                    "Company Id is required.",
                    nameof(companyId));

            var result = new ProjectComplianceResult();

            var wpsList = _wpsRepo.GetByCompany(companyId);
            var pqrList = _pqrRepo.GetByCompany(companyId);

            result.TotalWps = wpsList.Count;

            foreach (var wps in wpsList)
            {
                if (wps.PqrId == null)
                {
                    result.Issues.Add(
                        $"WPS {wps.WpsNumber}: No PQR linked");
                    continue;
                }

                var pqr = pqrList.FirstOrDefault(
                    x => x.Id == wps.PqrId.Value);

                if (pqr == null)
                {
                    result.Issues.Add(
                        $"WPS {wps.WpsNumber}: Invalid or cross-company PQR reference");
                    continue;
                }

                var compliance = _wpsCompliance.Evaluate(wps, pqr);

                if (compliance.IsCompliant)
                {
                    result.CompliantWps++;
                }
                else
                {
                    result.Issues.Add(
                        $"WPS {wps.WpsNumber}: Not compliant");
                }
            }

            // =========================
            // WELDERS
            // =========================
            var companyWelders =
                _welderQualificationRepo.GetByCompany(companyId);

            var expired =
                companyWelders.Count(
                    w => w.ExpiryDate < DateTime.Today);

            if (expired > 0)
            {
                result.Issues.Add(
                    $"{expired} welders expired");
            }

            // =========================
            // DOCUMENTS (ISO 3834 CORE)
            // =========================
            var docs = _docRepo.GetByProject(projectId);

            var requiredDocs =
                docs.Where(d => d.IsRequired).ToList();

            if (requiredDocs.Any())
            {
                var approved =
                    requiredDocs.Count(d => d.IsApproved);

                result.DocumentCompliancePercent =
                    (double)approved / requiredDocs.Count * 100;

                foreach (var doc in requiredDocs)
                {
                    if (!doc.IsUploaded)
                    {
                        result.Issues.Add(
                            $"Missing document: {doc.DocumentType}");
                    }

                    if (doc.IsUploaded && !doc.IsApproved)
                    {
                        result.Issues.Add(
                            $"Not approved: {doc.DocumentType}");
                    }
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

using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class ProjectDocumentService
    {
        private readonly ProjectDocumentRepository _repo = new();

        private readonly List<string> _defaultDocs = new()
        {
            "Index",
            "List of Involved Parties",
            "Method Statement",
            "Quality Control Plan",

            "Drawings",
            "WPS / PQR / WPQR",

            "Material Certificates",
            "Welding Consumable Certificates",

            "NDT Procedures",
            "NDT Reports",

            "NCR",

            "Pressure Test Certificates",

            "Certificate of Manufacture",

            "ISO 3834 Certificate"
        };

        public void InitializeProjectDocuments(Guid projectId)
        {
            var existing = _repo.GetByProject(projectId);

            if (existing.Any())
                return;

            foreach (var docType in _defaultDocs)
            {
                _repo.Add(new ProjectDocument
                {
                    Id = Guid.NewGuid(),          // 🔥 REQUIRED
                    ProjectId = projectId,
                    DocumentType = docType,
                    IsRequired = true,
                    IsUploaded = false
                });
            }
        }
    }
}
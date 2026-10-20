using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class ProjectDocumentService
    {
        private readonly ProjectDocumentRepository _repo = new();

        private readonly List<ProjectDocument> _defaultDocs = new()
{
    new()
    {
        DocumentType = "Index",
        Category = "Turnover",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Method Statement",
        Category = "Quality",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Quality Control Plan",
        Category = "Quality",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Drawings",
        Category = "Engineering",
        IsRequired = true
    },

    new()
    {
        DocumentType = "WPS",
        Category = "Quality",
        IsRequired = true,
        AllowMultiple = true
    },

    new()
    {
        DocumentType = "PQR",
        Category = "Quality",
        IsRequired = true,
        AllowMultiple = true
    },

    new()
    {
        DocumentType = "WPQR",
        Category = "Quality",
        IsRequired = true,
        AllowMultiple = true
    },

    new()
    {
        DocumentType = "Inspection Reports",
        Category = "Inspection",
        IsRequired = true,
        AllowMultiple = true
    },

    new()
    {
        DocumentType = "NDT Reports",
        Category = "Inspection",
        IsRequired = true,
        AllowMultiple = true
    },

    new()
    {
        DocumentType = "Material Certificates",
        Category = "Certificates",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Consumable Certificates",
        Category = "Certificates",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Weld Map",
        Category = "Engineering",
        IsRequired = true
    },

    new()
    {
        DocumentType = "Final Data Book",
        Category = "Turnover",
        IsRequired = true
    }
};

        public void InitializeProjectDocuments(Guid projectId)
        {
            if (projectId == Guid.Empty)
                return;

            var existing = _repo.GetByProject(projectId);

            if (existing.Any())
                return;

            foreach (var doc in _defaultDocs)
            {
                _repo.Add(new ProjectDocument
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    DocumentType = doc.DocumentType,
                    DocumentName = doc.DocumentType,
                    Category = doc.Category,
                    IsRequired = doc.IsRequired,
                    AllowMultiple = doc.AllowMultiple,
                    IsUploaded = false,
                    Revision = 0
                });
            }
        }
    }
}
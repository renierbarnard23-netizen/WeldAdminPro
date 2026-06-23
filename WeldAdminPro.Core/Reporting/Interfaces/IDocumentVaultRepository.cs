using PdfSharpCore.Pdf.IO;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Interfaces
{
    public interface IDocumentVaultRepository
    {
        void Add(
            DocumentVaultFile file);

        List<DocumentVaultFile> GetAll();

        List<DocumentVaultFile> GetByProject(
            Guid projectId);

        List<DocumentVaultFile> GetByWeld(
            Guid weldId);

        List<DocumentVaultFile>
            GetApprovedByCategory(
            DocumentCategoryType category);

    }
}

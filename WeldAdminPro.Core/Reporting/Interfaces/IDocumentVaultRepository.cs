using PdfSharpCore.Pdf.IO;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Interfaces
{
    public interface IDocumentVaultRepository
    {
        void Add(
            DocumentVaultFile file);

        void Approve(
            Guid id,
            string approvedBy);

        DocumentVaultFile? GetById(Guid id);

        List<DocumentVaultFile> GetAll();

        List<DocumentVaultFile> GetByProject(
            Guid projectId);

        List<DocumentVaultFile> GetByWeld(
            Guid weldId);

        List<DocumentVaultFile> GetByCompany(
            Guid companyId);

        List<DocumentVaultFile>
            GetApprovedByCompanyAndCategory(
            Guid companyId,
            DocumentCategoryType category);

        List<DocumentVaultFile>
            GetApprovedByCategory(
            DocumentCategoryType category);
    }
}


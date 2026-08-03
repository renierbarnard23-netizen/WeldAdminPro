using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class DocumentApplicationService
{
    private readonly ProjectDocumentRepository _repository;

    public DocumentApplicationService(
        ProjectDocumentRepository repository)
    {
        _repository = repository;
    }

    public int GetRequiredDocumentCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.IsRequired);
    }

    public int GetUploadedDocumentCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.IsUploaded);
    }

    public int GetApprovedDocumentCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.IsApproved);
    }

    public int GetMissingDocumentCount()
    {
        return _repository
            .GetAll()
            .Count(x =>
                x.IsRequired &&
                !x.IsUploaded);
    }
}
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.Projects;

namespace WeldAdminPro.Web.Services.Quality;

public class DocumentApplicationService
{
    private readonly ProjectDocumentRepository _repository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly ProjectApplicationService _projectService;

    public DocumentApplicationService(
        ProjectDocumentRepository repository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        ProjectApplicationService projectService)
    {
        _repository = repository;
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;
        _projectService = projectService;
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

    public async Task<int> GetMissingDocumentCountAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated user ID is required.");
        }

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.View);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view Quality document metrics.");
        }

        var accessibleProjects =
            await _projectService.GetProjectsAsync();

        var missingCount = 0;

        foreach (var project in accessibleProjects)
        {
            missingCount += _repository
                .GetByProject(project.Id)
                .Count(x =>
                    x.IsRequired &&
                    !x.IsUploaded);
        }

        return missingCount;
    }
}
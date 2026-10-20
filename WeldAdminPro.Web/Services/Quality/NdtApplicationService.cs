using System;
using System.Threading.Tasks;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Data.Services.Projects;

namespace WeldAdminPro.Web.Services.Quality;


public class NdtApplicationService
{
    private readonly WeldNdtRepository _repository;
    private readonly IWeldRepository _weldRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly ProjectApplicationService _projectService;

    public NdtApplicationService(
        WeldNdtRepository repository,
        IWeldRepository weldRepository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        ProjectApplicationService projectService)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));

        _weldRepository = weldRepository
            ?? throw new ArgumentNullException(nameof(weldRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _permissionAuthorization = permissionAuthorization
            ?? throw new ArgumentNullException(nameof(permissionAuthorization));

        _projectService = projectService
            ?? throw new ArgumentNullException(nameof(projectService));
    }


    public async Task<int> GetPendingCountAsync()
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authentication is required.");

        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException(
                "Authenticated user identity is required.");

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                userId,
                _currentUser.Role,
                PermissionKeys.Quality.NDT);

        if (!hasPermission)
            throw new UnauthorizedAccessException(
                "You do not have permission to view NDT quality data.");

        var accessibleProjects =
            await _projectService.GetProjectsAsync();

        var pendingCount = 0;

        foreach (var project in accessibleProjects)
        {
            var welds =
                await _weldRepository.GetByProjectAsync(project.Id);

            foreach (var weld in welds)
            {
                pendingCount += _repository
                    .GetByWeld(weld.Id)
                    .Count(x => x.Result == NdtResultType.Pending);
            }
        }

        return pendingCount;
    }

    public int GetAcceptedCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Accept);
    }

    public int GetRejectedCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Reject);
    }

    public int GetRepairCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Repair);
    }

    public int GetConditionalAcceptCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.ConditionalAccept);
    }
}

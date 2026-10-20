using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Data.Services.Security;

public class ProjectAccessAuthorizationService
    : IProjectAccessAuthorizationService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IUserCompanyAccessRepository _companyAccessRepository;
    private readonly IProjectUserAssignmentRepository _projectAssignmentRepository;

    public ProjectAccessAuthorizationService(
        ICurrentUserContext currentUser,
        IUserCompanyAccessRepository companyAccessRepository,
        IProjectUserAssignmentRepository projectAssignmentRepository)
    {
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _companyAccessRepository = companyAccessRepository
            ?? throw new ArgumentNullException(nameof(companyAccessRepository));

        _projectAssignmentRepository = projectAssignmentRepository
            ?? throw new ArgumentNullException(nameof(projectAssignmentRepository));
    }

    public async Task<bool> CanAccessCompanyAsync(
        string userId,
        Guid companyId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (companyId == Guid.Empty)
            return false;

        if (!string.Equals(
                _currentUser.UserId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsAdministrator())
            return true;

        if (_currentUser.CompanyId.HasValue &&
            _currentUser.CompanyId.Value == companyId)
        {
            return true;
        }

        return await _companyAccessRepository
            .HasActiveAccessAsync(userId, companyId);
    }

    public async Task<bool> CanAccessProjectAsync(
        string userId,
        Project project)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (project == null)
            return false;

        if (project.Id == Guid.Empty)
            return false;

        if (!string.Equals(
                _currentUser.UserId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsAdministrator())
            return true;

        if (!project.CompanyId.HasValue ||
            project.CompanyId.Value == Guid.Empty)
        {
            return false;
        }

        var companyId = project.CompanyId.Value;

        // Home-company users automatically have project access.
        if (_currentUser.CompanyId.HasValue &&
            _currentUser.CompanyId.Value == companyId)
        {
            return true;
        }

        // Additional-company access requires both:
        // 1. active company access
        // 2. active project assignment
        var hasCompanyAccess =
            await _companyAccessRepository
                .HasActiveAccessAsync(userId, companyId);

        if (!hasCompanyAccess)
            return false;

        return await _projectAssignmentRepository
            .HasActiveAssignmentAsync(userId, project.Id);
    }

    private bool IsAdministrator()
    {
        return string.Equals(
            _currentUser.Role,
            "Administrator",
            StringComparison.OrdinalIgnoreCase);
    }
}

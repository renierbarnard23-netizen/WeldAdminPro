using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Security.Abstractions;

public interface IProjectAccessAuthorizationService
{
    Task<bool> CanAccessCompanyAsync(
        string userId,
        Guid companyId);

    Task<bool> CanAccessProjectAsync(
        string userId,
        Project project);
}

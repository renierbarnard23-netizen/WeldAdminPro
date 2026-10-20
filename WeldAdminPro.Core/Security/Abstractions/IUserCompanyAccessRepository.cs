using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Core.Security.Abstractions;

public interface IUserCompanyAccessRepository
{
    Task<IEnumerable<UserCompanyAccess>> GetByUserIdAsync(
        string userId);

    Task<bool> HasActiveAccessAsync(
        string userId,
        Guid companyId);

    Task AddAsync(
        UserCompanyAccess access);

    Task DeactivateAsync(
        Guid id);
}

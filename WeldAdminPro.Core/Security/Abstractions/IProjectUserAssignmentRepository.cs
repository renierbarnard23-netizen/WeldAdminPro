using WeldAdminPro.Core.Security.Models;

namespace WeldAdminPro.Core.Security.Abstractions;

public interface IProjectUserAssignmentRepository
{
    Task<IEnumerable<ProjectUserAssignment>> GetByProjectIdAsync(
        Guid projectId);

    Task<IEnumerable<ProjectUserAssignment>> GetByUserIdAsync(
        string userId);

    Task<bool> HasActiveAssignmentAsync(
        string userId,
        Guid projectId);

    Task AddAsync(
        ProjectUserAssignment assignment);

    Task DeactivateAsync(
        Guid id);
}

using Dapper;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Security.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Repositories.Security;

public class ProjectUserAssignmentRepository
    : RepositoryBase,
      IProjectUserAssignmentRepository
{
    public ProjectUserAssignmentRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<ProjectUserAssignment>>
        GetByProjectIdAsync(Guid projectId)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));

        return await QueryAsync<ProjectUserAssignment>(
            @"
SELECT
    Id,
    ProjectId,
    UserId,
    RoleId,
    IsActive,
    ValidFrom,
    ValidUntil,
    CreatedOn,
    CreatedBy,
    Notes
FROM ProjectUserAssignment
WHERE ProjectId = @ProjectId
ORDER BY CreatedOn;",
            new
            {
                ProjectId = projectId
            });
    }

    public async Task<IEnumerable<ProjectUserAssignment>>
        GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        return await QueryAsync<ProjectUserAssignment>(
            @"
SELECT
    Id,
    ProjectId,
    UserId,
    RoleId,
    IsActive,
    ValidFrom,
    ValidUntil,
    CreatedOn,
    CreatedBy,
    Notes
FROM ProjectUserAssignment
WHERE UserId = @UserId
ORDER BY CreatedOn;",
            new
            {
                UserId = userId
            });
    }

    public async Task<bool> HasActiveAssignmentAsync(
        string userId,
        Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (projectId == Guid.Empty)
            return false;

        var now = DateTime.UtcNow;

        var count = await QuerySingleAsync<int>(
            @"
SELECT COUNT(*)
FROM ProjectUserAssignment
WHERE UserId = @UserId
  AND ProjectId = @ProjectId
  AND IsActive = 1
  AND (ValidFrom IS NULL OR ValidFrom <= @Now)
  AND (ValidUntil IS NULL OR ValidUntil >= @Now);",
            new
            {
                UserId = userId,
                ProjectId = projectId,
                Now = now
            });

        return count > 0;
    }

    public async Task AddAsync(
        ProjectUserAssignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        if (assignment.ProjectId == Guid.Empty)
            throw new ArgumentException(
                "Project ID is required.",
                nameof(assignment));

        if (string.IsNullOrWhiteSpace(assignment.UserId))
            throw new ArgumentException(
                "User ID is required.",
                nameof(assignment));

        if (assignment.Id == Guid.Empty)
            assignment.Id = Guid.NewGuid();

        if (assignment.CreatedOn == default)
            assignment.CreatedOn = DateTime.UtcNow;

        await ExecuteAsync(
            @"
INSERT INTO ProjectUserAssignment
(
    Id,
    ProjectId,
    UserId,
    RoleId,
    IsActive,
    ValidFrom,
    ValidUntil,
    CreatedOn,
    CreatedBy,
    Notes
)
VALUES
(
    @Id,
    @ProjectId,
    @UserId,
    @RoleId,
    @IsActive,
    @ValidFrom,
    @ValidUntil,
    @CreatedOn,
    @CreatedBy,
    @Notes
);",
            assignment);
    }

    public async Task DeactivateAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Assignment ID is required.",
                nameof(id));

        await ExecuteAsync(
            @"
UPDATE ProjectUserAssignment
SET IsActive = 0
WHERE Id = @Id;",
            new
            {
                Id = id
            });
    }
}

using Dapper;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Security.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Repositories.Security;

public class UserCompanyAccessRepository
    : RepositoryBase,
      IUserCompanyAccessRepository
{
    public UserCompanyAccessRepository(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<UserCompanyAccess>>
        GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        var rows = await QueryAsync<UserCompanyAccessRow>(
            @"
SELECT
    Id,
    UserId,
    CompanyId,
    IsActive,
    ValidFrom,
    ValidUntil,
    CreatedOn,
    CreatedBy,
    Notes
FROM UserCompanyAccess
WHERE UserId = @UserId
ORDER BY CreatedOn;",
            new
            {
                UserId = userId
            });

        return rows.Select(row => new UserCompanyAccess
        {
            Id = Guid.Parse(row.Id),
            UserId = row.UserId,
            CompanyId = Guid.Parse(row.CompanyId),
            IsActive = row.IsActive,
            ValidFrom = row.ValidFrom,
            ValidUntil = row.ValidUntil,
            CreatedOn = row.CreatedOn,
            CreatedBy = row.CreatedBy,
            Notes = row.Notes
        });
    }

    private sealed class UserCompanyAccessRow
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
    public async Task<bool> HasActiveAccessAsync(
        string userId,
        Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (companyId == Guid.Empty)
            return false;

        var now = DateTime.UtcNow;

        var count = await QuerySingleAsync<int>(
            @"
SELECT COUNT(*)
FROM UserCompanyAccess
WHERE UserId = @UserId
  AND CompanyId = @CompanyId
  AND IsActive = 1
  AND (ValidFrom IS NULL OR ValidFrom <= @Now)
  AND (ValidUntil IS NULL OR ValidUntil >= @Now);",
            new
            {
                UserId = userId,
                CompanyId = companyId,
                Now = now
            });

        return count > 0;
    }

    public async Task AddAsync(
        UserCompanyAccess access)
    {
        if (access == null)
            throw new ArgumentNullException(nameof(access));

        if (string.IsNullOrWhiteSpace(access.UserId))
            throw new ArgumentException(
                "User ID is required.",
                nameof(access));

        if (access.CompanyId == Guid.Empty)
            throw new ArgumentException(
                "Company ID is required.",
                nameof(access));

        if (access.Id == Guid.Empty)
            access.Id = Guid.NewGuid();

        if (access.CreatedOn == default)
            access.CreatedOn = DateTime.UtcNow;

        await ExecuteAsync(
            @"
INSERT INTO UserCompanyAccess
(
    Id,
    UserId,
    CompanyId,
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
    @UserId,
    @CompanyId,
    @IsActive,
    @ValidFrom,
    @ValidUntil,
    @CreatedOn,
    @CreatedBy,
    @Notes
);",
            access);
    }

    public async Task DeactivateAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Access ID is required.",
                nameof(id));

        await ExecuteAsync(
            @"
UPDATE UserCompanyAccess
SET IsActive = 0
WHERE Id = @Id;",
            new
            {
                Id = id
            });
    }
}


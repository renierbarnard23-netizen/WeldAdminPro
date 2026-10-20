using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class WelderQualificationApplicationService
{
    private readonly WelderQualificationRepository _repository;
    private readonly CompanyRepository _companyRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

    public WelderQualificationApplicationService(
        WelderQualificationRepository repository,
        CompanyRepository companyRepository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        IProjectAccessAuthorizationService projectAccessAuthorization)
    {
        _repository = repository;
        _companyRepository = companyRepository
            ?? throw new ArgumentNullException(nameof(companyRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _permissionAuthorization = permissionAuthorization
            ?? throw new ArgumentNullException(nameof(permissionAuthorization));

        _projectAccessAuthorization = projectAccessAuthorization
            ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
    }

    /// <summary>
    /// Returns welder qualifications belonging only to companies
    /// the current user is authorized to access.
    ///
    /// This read intentionally does not require the management
    /// permission because it is also used by selectors such as WPQR.
    /// </summary>
    public async Task<List<WelderQualification>> GetAccessibleAsync()
    {
        EnsureAuthenticated();

        var all = _repository.GetAll();

        var accessible = new List<WelderQualification>();

        foreach (var qualification in all)
        {
            if (!qualification.CompanyId.HasValue ||
                qualification.CompanyId.Value == Guid.Empty)
            {
                continue;
            }

            if (await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    qualification.CompanyId.Value))
            {
                accessible.Add(qualification);
            }
        }

        return accessible;
    }

    /// <summary>
    /// Returns all welder qualifications for management screens,
    /// restricted by the Welder Qualifications permission and
    /// company access.
    /// </summary>
    public async Task<List<WelderQualification>> GetAllAsync()
    {
        await EnsureManagementAccessAsync();

        var all = _repository.GetAll();

        var accessible = new List<WelderQualification>();

        foreach (var qualification in all)
        {
            if (!qualification.CompanyId.HasValue ||
                qualification.CompanyId.Value == Guid.Empty)
            {
                continue;
            }

            if (await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    qualification.CompanyId.Value))
            {
                accessible.Add(qualification);
            }
        }

        return accessible;
    }

    /// <summary>
    /// Returns active companies the current user is authorized to use
    /// when creating a welder qualification.
    /// </summary>
    public async Task<List<Company>> GetAccessibleCompaniesAsync()
    {
        await EnsureManagementAccessAsync();

        var accessible = new List<Company>();

        foreach (var company in _companyRepository.GetAll())
        {
            if (!company.IsActive || company.Id == Guid.Empty)
            {
                continue;
            }

            if (await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    company.Id))
            {
                accessible.Add(company);
            }
        }

        return accessible
            .OrderBy(x => x.CompanyName)
            .ToList();
    }

    public async Task CreateAsync(WelderQualification qualification)
    {
        await EnsureManagementAccessAsync();

        if (qualification == null)
        {
            throw new ArgumentNullException(nameof(qualification));
        }

        if (!qualification.CompanyId.HasValue ||
            qualification.CompanyId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "A company must be selected for the welder qualification.",
                nameof(qualification));
        }

        var company = _companyRepository.GetById(
            qualification.CompanyId.Value);

        if (company == null || !company.IsActive)
        {
            throw new InvalidOperationException(
                "The selected company does not exist or is inactive.");
        }

        await EnsureCompanyAccessAsync(qualification.CompanyId);

        Validate(qualification);

        if (qualification.InitialQualificationDate == DateTime.MinValue)
        {
            qualification.InitialQualificationDate =
                qualification.QualificationDate;
        }

        _repository.Add(qualification);
    }

    public async Task UpdateAsync(WelderQualification qualification)
    {
        await EnsureManagementAccessAsync();

        if (qualification.Id <= 0)
        {
            throw new ArgumentException(
                "The welder qualification ID is invalid.",
                nameof(qualification));
        }

        var existing = _repository.GetAll()
            .FirstOrDefault(x => x.Id == qualification.Id);

        if (existing == null)
        {
            throw new InvalidOperationException(
                "The welder qualification could not be found.");
        }

        await EnsureCompanyAccessAsync(existing.CompanyId);

        Validate(qualification);

        // Never trust CompanyId supplied by the UI.
        qualification.CompanyId = existing.CompanyId;

        _repository.Update(qualification);
    }

    public async Task DeleteAsync(int id)
    {
        await EnsureManagementAccessAsync();

        if (id <= 0)
        {
            throw new ArgumentException(
                "The welder qualification ID is invalid.",
                nameof(id));
        }

        var existing = _repository.GetAll()
            .FirstOrDefault(x => x.Id == id);

        if (existing == null)
        {
            throw new InvalidOperationException(
                "The welder qualification could not be found.");
        }

        await EnsureCompanyAccessAsync(existing.CompanyId);

        _repository.Delete(id);
    }

    public async Task RenewAsync(
        WelderQualification qualification,
        int months = 6)
    {
        await EnsureManagementAccessAsync();

        if (qualification == null)
        {
            throw new ArgumentNullException(nameof(qualification));
        }

        if (months <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(months),
                "Renewal period must be greater than zero.");
        }

        if (qualification.Id <= 0)
        {
            throw new ArgumentException(
                "The welder qualification ID is invalid.",
                nameof(qualification));
        }

        var existing = _repository.GetAll()
            .FirstOrDefault(x => x.Id == qualification.Id);

        if (existing == null)
        {
            throw new InvalidOperationException(
                "The welder qualification could not be found.");
        }

        await EnsureCompanyAccessAsync(existing.CompanyId);

        if (!existing.CompanyId.HasValue ||
            existing.CompanyId.Value == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The welder qualification has no valid company ownership.");
        }

        var renewalDate = DateTime.Today;
        var previousExpiryDate = existing.ExpiryDate;
        var newExpiryDate = renewalDate.AddMonths(months);
        var renewedByUserId = _currentUser.UserId;
        var renewedByUsername = _currentUser.Username;
        var renewedByName = _currentUser.FullName;

        if (string.IsNullOrWhiteSpace(renewedByUserId))
        {
            throw new UnauthorizedAccessException(
                "The authenticated user ID is required for renewal auditing.");
        }

        if (string.IsNullOrWhiteSpace(renewedByName))
        {
            renewedByName = renewedByUsername;

            if (string.IsNullOrWhiteSpace(renewedByName))
            {
                throw new UnauthorizedAccessException(
                    "The authenticated user name is required for renewal auditing.");
            }
        }

        var renewedAt = DateTime.Now;

        _repository.RenewWithHistory(
            qualificationId: existing.Id,
            companyId: existing.CompanyId,
            welderNumber: existing.WelderNumber,
            previousExpiryDate: previousExpiryDate,
            newExpiryDate: newExpiryDate,
            renewalDate: renewalDate,
            renewalPeriodMonths: months,
            renewedByUserId: renewedByUserId,
            renewedByUsername: renewedByUsername,
            renewedByName: renewedByName,
            lastRenewedByUserId: renewedByUserId,
            lastRenewedByName: renewedByName,
            lastRenewedAt: renewedAt,
            previousExpiryDateAudit: previousExpiryDate);

        qualification.CompanyId = existing.CompanyId;
        qualification.RenewalDate = renewalDate;
        qualification.ExpiryDate = newExpiryDate;
        qualification.IsActive = true;
        qualification.LastRenewedByUserId = renewedByUserId;
        qualification.LastRenewedByName = renewedByName;
        qualification.LastRenewedAt = renewedAt;
        qualification.PreviousExpiryDate = previousExpiryDate;
    }

    public async Task<int> GetValidQualifiedWelderCountAsync()
    {
        EnsureAuthenticated();

        var qualifications = await GetAccessibleAsync();

        return qualifications
            .Where(x => x.ExpiryDate >= DateTime.Today)
            .Select(x => x.WelderNumber.Trim().ToUpper())
            .Distinct()
            .Count();
    }

    public async Task<int> GetExpiringQualificationCountAsync(
        int days = 30)
    {
        EnsureAuthenticated();

        var today = DateTime.Today;
        var expiryLimit = today.AddDays(days);

        var qualifications = await GetAccessibleAsync();

        return qualifications.Count(x =>
            x.ExpiryDate >= today &&
            x.ExpiryDate <= expiryLimit);
    }

    private async Task EnsureManagementAccessAsync()
    {
        EnsureAuthenticated();

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WelderQualifications);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to manage welder qualifications.");
        }
    }

    private async Task EnsureCompanyAccessAsync(Guid? companyId)
    {
        if (!companyId.HasValue ||
            companyId.Value == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The welder qualification has no valid company ownership.");
        }

        var hasAccess =
            await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                companyId.Value);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company owning this welder qualification.");
        }
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "An authenticated user is required.");
        }
    }

    private static void Validate(WelderQualification qualification)
    {
        if (qualification == null)
        {
            throw new ArgumentNullException(nameof(qualification));
        }

        if (string.IsNullOrWhiteSpace(qualification.WelderNumber))
            throw new ArgumentException("Welder number is required.");

        if (string.IsNullOrWhiteSpace(qualification.Process))
            throw new ArgumentException("Process is required.");

        if (string.IsNullOrWhiteSpace(qualification.Position))
            throw new ArgumentException("Position is required.");

        if (qualification.ExpiryDate <= qualification.QualificationDate)
            throw new ArgumentException(
                "Expiry date must be after qualification date.");

        if (qualification.ThicknessMin < 0)
            throw new ArgumentException(
                "Minimum thickness cannot be negative.");

        if (qualification.ThicknessMax < qualification.ThicknessMin)
            throw new ArgumentException(
                "Maximum thickness cannot be less than minimum thickness.");
    }
}



using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class WpsApplicationService
{
    private readonly WpsRepository _wpsRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly PqrRepository _pqrRepository;
    private readonly WpsGeneratorService _generator;
    private readonly WpsComplianceService _compliance;

    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
    private readonly CompanyRepository _companyRepository;

    public WpsApplicationService(
        WpsRepository wpsRepository,
        PqrRepository pqrRepository,
        ProjectRepository projectRepository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        IProjectAccessAuthorizationService projectAccessAuthorization,
        CompanyRepository companyRepository)
    {
        _wpsRepository = wpsRepository;
        _pqrRepository = pqrRepository;
        _projectRepository = projectRepository;

        _currentUser =
            currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _permissionAuthorization =
            permissionAuthorization
            ?? throw new ArgumentNullException(nameof(permissionAuthorization));

        _projectAccessAuthorization =
            projectAccessAuthorization
            ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));

        _companyRepository =
            companyRepository
            ?? throw new ArgumentNullException(nameof(companyRepository));

        _generator = new WpsGeneratorService();
        _compliance = new WpsComplianceService();
    }

    public async Task<List<Company>> GetAccessibleCompaniesAsync()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access WPS records.");
        }

        var result = new List<Company>();

        foreach (var company in _companyRepository.GetAll())
        {
            if (company.IsActive &&
                await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    company.Id))
            {
                result.Add(company);
            }
        }

        return result;
    }

    private async Task EnsureWpsReadAccessAsync(Guid companyId)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access WPS records.");
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                companyId))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company that owns this WPS.");
        }
    }

    private async Task EnsureWpsMutationAccessAsync(
        Guid? companyId,
        bool requireExistingOwnership)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to modify WPS records.");
        }

        if (!companyId.HasValue || companyId.Value == Guid.Empty)
        {
            if (requireExistingOwnership)
            {
                throw new InvalidOperationException(
                    "The WPS does not have a valid owning company.");
            }

            if (!_currentUser.CompanyId.HasValue ||
                _currentUser.CompanyId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Your user account does not have a company.");
            }

            return;
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                companyId.Value))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company that owns this WPS.");
        }
    }

    // =====================================================
    // WPS QUERIES
    // =====================================================

    /// <summary>
    /// Returns approved and active WPS revisions applicable to the
    /// welding company assigned to the project.
    /// </summary>
    public async Task<List<Wps>> GetForProjectAsync(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var project = _projectRepository.GetById(projectId);

        if (project == null)
        {
            throw new KeyNotFoundException(
                "The project was not found.");
        }

        if (!await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this project.");
        }

        var weldingCompanyId =
            project.WeldingSubcontractorCompanyId
            ?? project.CompanyId;

        if (!weldingCompanyId.HasValue ||
            weldingCompanyId.Value == Guid.Empty)
        {
            return new List<Wps>();
        }

        await EnsureWpsReadAccessAsync(weldingCompanyId.Value);

        return _wpsRepository
            .GetByCompany(weldingCompanyId.Value)
            .Where(wps =>
                wps.IsApproved &&
                wps.IsActive)
            .OrderBy(wps => wps.WpsNumber)
            .ThenByDescending(wps => wps.Revision)
            .ToList();
    }

    public async Task<List<Wps>> GetAllAsync()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access WPS records.");
        }

        var all = _wpsRepository.GetAll();
        var result = new List<Wps>();

        foreach (var wps in all)
        {
            if (wps.CompanyId.HasValue &&
                await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    wps.CompanyId.Value))
            {
                result.Add(wps);
            }
        }

        return result;
    }

    public async Task<List<Wps>> GetActiveAsync()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access WPS records.");
        }

        var all = _wpsRepository.GetActive();
        var result = new List<Wps>();

        foreach (var wps in all)
        {
            if (wps.CompanyId.HasValue &&
                await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    wps.CompanyId.Value))
            {
                result.Add(wps);
            }
        }

        return result;
    }

    public async Task<Wps?> GetAsync(string wpsNumber)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access WPS records.");
        }

        var wps =
            _wpsRepository.GetByWpsNumber(wpsNumber);

        if (wps == null ||
            !wps.CompanyId.HasValue)
        {
            return null;
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                wps.CompanyId.Value))
        {
            return null;
        }

        return wps;
    }

    // =====================================================
    // PQR QUERIES
    // =====================================================

    public async Task<List<Pqr>> GetAvailablePqrsAsync(Guid? companyId)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access PQR records through WPS.");
        }

        if (!companyId.HasValue || companyId.Value == Guid.Empty)
        {
            return new List<Pqr>();
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                companyId.Value))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the selected company.");
        }

        var result = new List<Pqr>();

        foreach (var pqr in _pqrRepository.GetAll())
        {
            if (!pqr.CompanyId.HasValue ||
                pqr.CompanyId.Value != companyId.Value ||
                !pqr.IsApproved ||
                !pqr.IsActive)
            {
                continue;
            }

            result.Add(pqr);
        }

        return result;
    }

    public async Task<Pqr?> GetPqrAsync(Guid id)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access PQR records through WPS.");
        }

        var pqr = _pqrRepository.GetById(id);

        if (pqr == null ||
            !pqr.CompanyId.HasValue ||
            pqr.CompanyId.Value == Guid.Empty)
        {
            return null;
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                pqr.CompanyId.Value))
        {
            return null;
        }

        return pqr;
    }

    // =====================================================
    // GENERATION
    // =====================================================

    public async Task<Wps> GenerateFromPqrAsync(Pqr pqr)
    {
        if (pqr == null)
            throw new ArgumentNullException(nameof(pqr));

        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to create WPS records.");
        }

        var authoritativePqr = _pqrRepository.GetById(pqr.Id);

        if (authoritativePqr == null)
        {
            throw new InvalidOperationException(
                "The selected PQR could not be found.");
        }

        if (!authoritativePqr.CompanyId.HasValue ||
            authoritativePqr.CompanyId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The selected PQR does not have a valid owning company.");
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                authoritativePqr.CompanyId.Value))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company that owns the selected PQR.");
        }

        return _generator.GenerateFromPqr(authoritativePqr);
    }

    // =====================================================
    // COMPLIANCE
    // =====================================================

    public WpsComplianceResult EvaluateCompliance(
        Wps wps,
        Pqr pqr)
    {
        return _compliance.Evaluate(wps, pqr);
    }

    // =====================================================
    // SAVE
    // =====================================================

    private async Task EnsurePqrOwnershipAsync(
        Guid? wpsCompanyId,
        Guid? pqrId)
    {
        if (!pqrId.HasValue || pqrId.Value == Guid.Empty)
            return;

        if (!wpsCompanyId.HasValue ||
            wpsCompanyId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The WPS does not have a valid owning company.");
        }

        var pqr = _pqrRepository.GetById(pqrId.Value);

        if (pqr == null)
        {
            throw new InvalidOperationException(
                "The linked PQR could not be found.");
        }

        if (!pqr.CompanyId.HasValue ||
            pqr.CompanyId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The linked PQR does not have a valid owning company.");
        }

        if (!pqr.IsApproved || !pqr.IsActive)
        {
            throw new InvalidOperationException(
                "The linked PQR must be approved and active.");
        }

        if (pqr.CompanyId.Value != wpsCompanyId.Value)
        {
            throw new InvalidOperationException(
                "The linked PQR must belong to the same company as the WPS.");
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                pqr.CompanyId.Value))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company that owns the linked PQR.");
        }
    }
    public async Task SaveAsync(Wps wps)
    {
        if (wps == null)
            throw new ArgumentNullException(nameof(wps));

        // Always reload the authoritative current revision from the repository.
        var existing =
            _wpsRepository.GetByWpsNumber(wps.WpsNumber);

        if (existing == null)
        {
            if (!wps.CompanyId.HasValue ||
                wps.CompanyId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Select an owning company for the new WPS.");
            }

            await EnsureWpsMutationAccessAsync(
                wps.CompanyId,
                requireExistingOwnership: false);

            var selectedCompany =
                _companyRepository.GetById(wps.CompanyId.Value);

            if (selectedCompany == null || !selectedCompany.IsActive)
            {
                throw new InvalidOperationException(
                    "The selected WPS company does not exist or is inactive.");
            }
        }
        else
        {
            await EnsureWpsMutationAccessAsync(
                existing.CompanyId,
                requireExistingOwnership: true);
        }

        // The WPS and its linked PQR must share the same
        // authoritative company ownership.
        var authoritativeWpsCompanyId =
            existing?.CompanyId ?? wps.CompanyId;

        await EnsurePqrOwnershipAsync(
            authoritativeWpsCompanyId,
            wps.PqrId);

        // Server-side WPS compliance must pass before saving.
        // Reload the linked PQR; do not trust a UI-supplied object.
        if (!wps.PqrId.HasValue || wps.PqrId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid linked PQR is required to save a WPS.");
        }

        var authoritativePqr =
            _pqrRepository.GetById(wps.PqrId.Value);

        if (authoritativePqr == null)
        {
            throw new InvalidOperationException(
                "The linked PQR could not be found.");
        }

        if (!authoritativePqr.IsApproved || !authoritativePqr.IsActive)
        {
            throw new InvalidOperationException(
                "The linked PQR must be approved and active.");
        }

        if (!authoritativeWpsCompanyId.HasValue ||
            !authoritativePqr.CompanyId.HasValue ||
            authoritativePqr.CompanyId.Value != authoritativeWpsCompanyId.Value)
        {
            throw new InvalidOperationException(
                "The WPS and linked PQR must belong to the same company.");
        }

        var complianceResult =
            EvaluateCompliance(wps, authoritativePqr);

        if (!complianceResult.IsCompliant)
        {
            var details = string.Join(
                Environment.NewLine,
                complianceResult.ValidationErrors
                    .Concat(complianceResult.EssentialFailures)
                    .Concat(complianceResult.RangeFailures)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct());

            throw new InvalidOperationException(
                "The WPS failed server-side compliance validation. " +
                (string.IsNullOrWhiteSpace(details)
                    ? "Review the WPS and linked PQR qualification."
                    : details));
        }

        // ---------------------------------------------------------
        // NEW WPS
        // ---------------------------------------------------------

        if (existing == null)
        {
            if (wps.Id == Guid.Empty)
                wps.Id = Guid.NewGuid();

            if (wps.Revision < 1)
                wps.Revision = 1;

            wps.IsActive = true;

            _wpsRepository.Add(wps);
            return;
        }

        // ---------------------------------------------------------
        // EXISTING APPROVED WPS
        //
        // Approved revisions are immutable.
        // Editing an approved WPS creates a new revision.
        // ---------------------------------------------------------

        if (existing.IsApproved)
        {
            var revision = existing.CreateRevision(
                existing.CreatedBy);

            // Revision ownership must remain identical to the
            // authoritative existing WPS ownership.
            revision.CompanyId = existing.CompanyId;

            // Apply the edited WPS data onto the new revision.
            revision.WpsNumber = wps.WpsNumber;
            revision.Process = wps.Process;
            revision.ThicknessMin = wps.ThicknessMin;
            revision.ThicknessMax = wps.ThicknessMax;
            revision.Diameter = wps.Diameter;
            revision.FillerMaterial = wps.FillerMaterial;
            revision.GasType = wps.GasType;
            revision.AmpsMin = wps.AmpsMin;
            revision.AmpsMax = wps.AmpsMax;
            revision.VoltsMin = wps.VoltsMin;
            revision.VoltsMax = wps.VoltsMax;
            revision.HeatInputMin = wps.HeatInputMin;
            revision.HeatInputMax = wps.HeatInputMax;
            revision.PreheatMin = wps.PreheatMin;
            revision.PreheatMax = wps.PreheatMax;
            revision.InterpassMax = wps.InterpassMax;
            revision.PwhtRequired = wps.PwhtRequired;
            revision.PqrId = wps.PqrId;
            revision.LinkedPqrNumber = wps.LinkedPqrNumber;
            revision.PNumber = wps.PNumber;
            revision.FNumber = wps.FNumber;
            revision.Position = wps.Position;
            revision.MaterialGroup = wps.MaterialGroup;
            revision.JointType = wps.JointType;
            revision.JointDesign = wps.JointDesign;
            revision.Backing = wps.Backing;
            revision.BackingType = wps.BackingType;
            revision.IsCompliant = wps.IsCompliant;
            revision.ValidationStatus = wps.ValidationStatus;
            revision.CurrentType = wps.CurrentType;
            revision.Polarity = wps.Polarity;
            revision.Progression = wps.Progression;
            revision.JointDiagramPath = wps.JointDiagramPath;
            revision.PassDiagramPath = wps.PassDiagramPath;
            revision.QualifiedThicknessRange = wps.QualifiedThicknessRange;
            revision.ParseConfidence = wps.ParseConfidence;
            revision.ParseNotes = wps.ParseNotes;

            // New revisions must start as active, unlocked and unapproved.
            revision.IsApproved = false;
            revision.IsLocked = false;
            revision.ApprovedBy = null;
            revision.ApprovedOn = null;
            revision.IsActive = true;

            // Save the previous and new revision atomically.
            // The repository transaction guarantees that either both
            // changes succeed or neither change is committed.
            _wpsRepository.SaveRevision(existing, revision);
            return;
        }

        // ---------------------------------------------------------
        // EXISTING UNAPPROVED WPS
        //
        // Normal editing remains an update.
        // ---------------------------------------------------------

        wps.Id = existing.Id;
        wps.Revision = existing.Revision;
        wps.IsActive = existing.IsActive;
        wps.PreviousRevisionId = existing.PreviousRevisionId;

        // Existing WPS ownership is authoritative.
        wps.CompanyId = existing.CompanyId;

        _wpsRepository.Update(wps);
    }

    // =====================================================
    // SAVE WITH COMPLIANCE CHECK
    // =====================================================

    public async Task<WpsComplianceResult> ValidateAndSaveAsync(
        Wps wps,
        Pqr pqr)
    {
        var result =
            EvaluateCompliance(wps, pqr);

        if (!result.IsCompliant)
            return result;

        await SaveAsync(wps);

        return result;
    }

    // =====================================================
    // DELETE
    // =====================================================

    public async Task DeleteAsync(Guid id)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var hasPermission =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WPS);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to delete WPS records.");
        }

        var existing =
            _wpsRepository.GetByWpsNumber(
                _wpsRepository
                    .GetAll()
                    .FirstOrDefault(x => x.Id == id)
                    ?.WpsNumber
                ?? string.Empty);

        if (existing == null)
        {
            throw new InvalidOperationException(
                "The WPS could not be found.");
        }

        if (!existing.CompanyId.HasValue ||
            existing.CompanyId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The WPS does not have a valid owning company.");
        }

        if (!await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                existing.CompanyId.Value))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to the company that owns this WPS.");
        }

        _wpsRepository.Delete(id);
    }
}












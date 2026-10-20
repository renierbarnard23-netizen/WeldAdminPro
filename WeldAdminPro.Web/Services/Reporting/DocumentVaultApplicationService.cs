using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Interfaces;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Web.Services.Reporting;

public class DocumentVaultApplicationService
{
    private readonly IDocumentVaultRepository _repository;
    private readonly DocumentStorageService _storage;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
    private readonly IWeldRepository _weldRepository;
    private readonly ProjectRepository _projectRepository;

    public DocumentVaultApplicationService(
        IDocumentVaultRepository repository,
        DocumentStorageService storage,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        IProjectAccessAuthorizationService projectAccessAuthorization,
        IWeldRepository weldRepository,
        ProjectRepository projectRepository)
    {
        _repository = repository;
        _storage = storage;
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;
        _projectAccessAuthorization = projectAccessAuthorization
            ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
        _weldRepository = weldRepository
            ?? throw new ArgumentNullException(nameof(weldRepository));
        _projectRepository = projectRepository
            ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public async Task<DocumentVaultFile> UploadWeldDocumentAsync(
        Guid weldId,
        Guid projectId,
        string fileName,
        DocumentCategoryType category,
        Stream stream,
        string? title = null,
        string? description = null,
        string? documentNumber = null,
        string? revision = null)
    {
        EnsureAuthenticated();

        if (weldId == Guid.Empty)
            throw new ArgumentException(
                "Weld ID is required.",
                nameof(weldId));

        if (projectId == Guid.Empty)
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));

        var weld = await _weldRepository.GetByIdAsync(weldId);

        if (weld == null)
        {
            throw new InvalidOperationException(
                "Weld could not be found.");
        }

        if (weld.ProjectId != projectId)
        {
            throw new UnauthorizedAccessException(
                "The weld does not belong to the specified project.");
        }

        var project = _projectRepository.GetById(weld.ProjectId);

        if (project == null)
        {
            throw new UnauthorizedAccessException(
                "The weld's project could not be found.");
        }

        var hasProjectAccess =
            await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project);

        if (!hasProjectAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this project.");
        }

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.View);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to access '{PermissionKeys.Quality.View}'.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var safeFileName =
            Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException(
                "A valid file name is required.",
                nameof(fileName));

        var filePath =
            await _storage.SaveWeldDocumentAsync(
                weldId,
                safeFileName,
                stream);

        var document = new DocumentVaultFile
        {
            Id = Guid.NewGuid(),
            FileName = safeFileName,
            OriginalFileName = safeFileName,
            FilePath = filePath,
            Category = category,
            Title = title?.Trim() ?? "",
            Description = description?.Trim() ?? "",
            DocumentNumber = documentNumber?.Trim() ?? "",
            Revision = revision?.Trim() ?? "",
            Status = "Pending",
            UploadedDate = DateTime.UtcNow,
            UploadedBy = _currentUser.Username.Trim(),
            WeldId = weldId,
            ProjectId = projectId,
            IsApproved = false
        };

        _repository.Add(document);

        return document;
    }

    public async Task ApproveWeldDocumentAsync(
        Guid documentId)
    {
        EnsureAuthenticated();

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WeldRelease);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to approve weld release documents.");
        }

        if (documentId == Guid.Empty)
            throw new ArgumentException(
                "Document ID is required.",
                nameof(documentId));

        _repository.Approve(
            documentId,
            _currentUser.Username.Trim());
    }

    public async Task<List<DocumentVaultFile>> GetWeldDocumentsAsync(
        Guid weldId,
        Guid projectId)
    {
        EnsureAuthenticated();

        if (weldId == Guid.Empty)
        {
            throw new ArgumentException(
                "Weld ID is required.",
                nameof(weldId));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        var weld = await _weldRepository.GetByIdAsync(weldId);

        if (weld == null)
        {
            throw new InvalidOperationException(
                "Weld could not be found.");
        }

        if (weld.ProjectId != projectId)
        {
            throw new UnauthorizedAccessException(
                "The weld does not belong to the specified project.");
        }

        var project = _projectRepository.GetById(weld.ProjectId);

        if (project == null)
        {
            throw new UnauthorizedAccessException(
                "The weld's project could not be found.");
        }

        var hasProjectAccess =
            await _projectAccessAuthorization.CanAccessProjectAsync(
                _currentUser.UserId,
                project);

        if (!hasProjectAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this project.");
        }

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.View);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to access '{PermissionKeys.Quality.View}'.");
        }

        return _repository.GetByWeld(weldId);
    }
    public List<DocumentVaultFile> GetWeldDocuments(
        Guid weldId)
    {
        if (weldId == Guid.Empty)
            throw new ArgumentException(
                "Weld ID is required.",
                nameof(weldId));

        return _repository.GetByWeld(weldId);
    }

    public async Task<DocumentVaultFile> UploadCompanyDocumentAsync(
        Guid companyId,
        string fileName,
        DocumentCategoryType category,
        Stream stream,
        string? title = null,
        string? description = null,
        string? documentNumber = null,
        string? revision = null)
    {
        await EnsureCompanyAccessAsync(
            companyId,
            GetCompanyDocumentPermission(category));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var safeFileName =
            Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException(
                "A valid file name is required.",
                nameof(fileName));

        var filePath =
            await _storage.SaveCompanyDocumentAsync(
                companyId,
                safeFileName,
                stream);

        var document = new DocumentVaultFile
        {
            Id = Guid.NewGuid(),
            FileName = safeFileName,
            OriginalFileName = safeFileName,
            FilePath = filePath,
            Category = category,
            Title = title?.Trim() ?? "",
            Description = description?.Trim() ?? "",
            DocumentNumber = documentNumber?.Trim() ?? "",
            Revision = revision?.Trim() ?? "",
            Status = "Pending",
            UploadedDate = DateTime.UtcNow,
            UploadedBy = _currentUser.Username.Trim(),
            WeldId = null,
            ProjectId = null,
            CompanyId = companyId,
            IsApproved = false
        };

        _repository.Add(document);

        return document;
    }

    public async Task<List<DocumentVaultFile>> GetCompanyDocumentsAsync(
        Guid companyId,
        DocumentCategoryType category)
    {
        await EnsureCompanyAccessAsync(
            companyId,
            GetCompanyDocumentPermission(category));

        return _repository
            .GetByCompany(companyId)
            .Where(x => x.Category == category)
            .ToList();
    }

    public async Task<List<DocumentVaultFile>> GetApprovedCompanyDocumentsAsync(
        Guid companyId,
        DocumentCategoryType category)
    {
        await EnsureCompanyAccessAsync(
            companyId,
            GetCompanyDocumentPermission(category));

        return _repository
            .GetApprovedByCompanyAndCategory(
                companyId,
                category);
    }

    public async Task ApproveCompanyDocumentAsync(
        Guid documentId)
    {
        EnsureAuthenticated();

        var documentForApproval =
            _repository.GetById(documentId);

        if (documentForApproval == null)
        {
            throw new InvalidOperationException(
                "Document could not be found.");
        }

        var approvalPermission =
            documentForApproval.Category == DocumentCategoryType.CalibrationCertificate
                ? PermissionKeys.Quality.Calibration
                : PermissionKeys.Quality.WpsApproval;

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                approvalPermission);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to approve this Quality Master document.");
        }

        if (documentId == Guid.Empty)
            throw new ArgumentException(
                "Document ID is required.",
                nameof(documentId));

        var document =
            documentForApproval;

        if (document == null)
        {
            throw new InvalidOperationException(
                "Document could not be found.");
        }

        if (!document.CompanyId.HasValue ||
            document.CompanyId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Company ownership is required before a company document can be approved.");
        }

        var hasCompanyAccess =
            await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                document.CompanyId.Value);

        if (!hasCompanyAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this company.");
        }

        _repository.Approve(
            documentId,
            _currentUser.Username.Trim());
    }

    private string GetCompanyDocumentPermission(
        DocumentCategoryType category)
    {
        return category switch
        {
            DocumentCategoryType.WPS =>
                PermissionKeys.Quality.WPS,

            DocumentCategoryType.WPQR =>
                PermissionKeys.Quality.WPQR,

            DocumentCategoryType.PQR =>
                PermissionKeys.Quality.PQR,

            DocumentCategoryType.CalibrationCertificate =>
                PermissionKeys.Quality.Calibration,

            _ =>
                PermissionKeys.Quality.View
        };
    }

    private async Task EnsureCompanyAccessAsync(
        Guid companyId,
        string permission)
    {
        EnsureAuthenticated();

        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Company ID is required.",
                nameof(companyId));
        }

        var hasCompanyAccess =
            await _projectAccessAuthorization.CanAccessCompanyAsync(
                _currentUser.UserId,
                companyId);

        if (!hasCompanyAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this company.");
        }

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                permission);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to access '{permission}'.");
        }
    }

    private void EnsureAuthenticated()
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

        if (string.IsNullOrWhiteSpace(_currentUser.Username))
        {
            throw new UnauthorizedAccessException(
                "Authenticated username is required.");
        }
    }
}















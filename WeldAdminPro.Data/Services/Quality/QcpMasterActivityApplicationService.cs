using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality;

public class QcpMasterActivityApplicationService
{
    private readonly QcpMasterActivityRepository _repository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;
    private readonly AuditService _auditService;

    public QcpMasterActivityApplicationService(
        QcpMasterActivityRepository repository,
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization,
        AuditService auditService)
    {
        _repository = repository;
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;
        _auditService = auditService;
    }

    public async Task<List<QcpMasterActivity>> GetAllAsync()
    {
        await EnsurePermissionAsync();
        return _repository.GetAll();
    }

    public async Task<List<QcpMasterActivity>> GetActiveAsync()
    {
        await EnsurePermissionAsync();
        return _repository.GetActive();
    }

    public async Task<QcpMasterActivity?> GetByIdAsync(Guid id)
    {
        await EnsurePermissionAsync();

        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "QCP Master Activity ID cannot be empty.",
                nameof(id));
        }

        return _repository.GetById(id);
    }

    public async Task AddAsync(QcpMasterActivity activity)
    {
        if (activity == null)
        {
            throw new ArgumentNullException(nameof(activity));
        }

        await EnsurePermissionAsync();
        EnsureAuthenticatedUser();

        if (activity.Id == Guid.Empty)
        {
            activity.Id = Guid.NewGuid();
        }

        Validate(activity);
        EnsureUniqueCode(activity.Code, null);

        activity.Code = activity.Code.Trim();
        activity.Name = activity.Name.Trim();
        activity.Category = activity.Category.Trim();
        activity.CreatedBy = _currentUser.Username!;
        activity.CreatedOn = DateTime.UtcNow;
        activity.ModifiedBy = string.Empty;
        activity.ModifiedOn = null;
        activity.IsActive = true;

        _repository.Add(activity);

        _auditService.Log(
            "QCP Master Activity Added",
            "Quality.QCPMaster",
            $"QCP Master Activity {activity.Code} ({activity.Name}) was added.");
    }

    public async Task UpdateAsync(QcpMasterActivity activity)
    {
        if (activity == null)
        {
            throw new ArgumentNullException(nameof(activity));
        }

        await EnsurePermissionAsync();
        EnsureAuthenticatedUser();

        if (activity.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "QCP Master Activity ID cannot be empty.",
                nameof(activity));
        }

        var existing = _repository.GetById(activity.Id);

        if (existing == null)
        {
            throw new KeyNotFoundException(
                "QCP Master Activity was not found.");
        }

        Validate(activity);
        EnsureUniqueCode(activity.Code, activity.Id);

        activity.Code = activity.Code.Trim();
        activity.Name = activity.Name.Trim();
        activity.Category = activity.Category.Trim();
        activity.CreatedBy = existing.CreatedBy;
        activity.CreatedOn = existing.CreatedOn;
        activity.ModifiedBy = _currentUser.Username!;
        activity.ModifiedOn = DateTime.UtcNow;
        activity.IsActive = existing.IsActive;

        _repository.Update(activity);

        _auditService.Log(
            "QCP Master Activity Updated",
            "Quality.QCPMaster",
            $"QCP Master Activity {activity.Code} ({activity.Name}) was updated.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        await EnsurePermissionAsync();
        EnsureAuthenticatedUser();

        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "QCP Master Activity ID cannot be empty.",
                nameof(id));
        }

        var existing = _repository.GetById(id);

        if (existing == null)
        {
            throw new KeyNotFoundException(
                "QCP Master Activity was not found.");
        }

        if (!existing.IsActive)
        {
            throw new InvalidOperationException(
                "QCP Master Activity is already inactive.");
        }

        var modifiedOn = DateTime.UtcNow;

        _repository.Deactivate(
            existing.Id,
            _currentUser.Username!,
            modifiedOn);

        _auditService.Log(
            "QCP Master Activity Deactivated",
            "Quality.QCPMaster",
            $"QCP Master Activity {existing.Code} ({existing.Name}) was deactivated.");
    }

    private async Task EnsurePermissionAsync()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException(
                "Current user identity could not be established.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Role))
        {
            throw new UnauthorizedAccessException(
                "User does not have an assigned role.");
        }

        var hasPermission = await _permissionAuthorization.HasPermissionAsync(
            _currentUser.UserId,
            _currentUser.Role,
            PermissionKeys.Quality.QCPMaster);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to manage the QCP Master Activity Library.");
        }
    }

    private void EnsureAuthenticatedUser()
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.Username))
        {
            throw new UnauthorizedAccessException(
                "The current user could not be established.");
        }
    }

    private void EnsureUniqueCode(string code, Guid? excludeId)
    {
        var normalizedCode = code.Trim();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return;
        }

        var duplicate = _repository.GetAll()
            .FirstOrDefault(x =>
                (!excludeId.HasValue || x.Id != excludeId.Value) &&
                string.Equals(
                    x.Code.Trim(),
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase));

        if (duplicate != null)
        {
            throw new InvalidOperationException(
                $"A QCP Master Activity with code '{normalizedCode}' already exists.");
        }
    }

    private static void Validate(QcpMasterActivity activity)
    {
        if (string.IsNullOrWhiteSpace(activity.Code))
        {
            throw new ArgumentException(
                "QCP Master Activity code is required.",
                nameof(activity));
        }

        if (string.IsNullOrWhiteSpace(activity.Name))
        {
            throw new ArgumentException(
                "QCP Master Activity name is required.",
                nameof(activity));
        }

        if (string.IsNullOrWhiteSpace(activity.Category))
        {
            throw new ArgumentException(
                "QCP Master Activity category is required.",
                nameof(activity));
        }
    }
}


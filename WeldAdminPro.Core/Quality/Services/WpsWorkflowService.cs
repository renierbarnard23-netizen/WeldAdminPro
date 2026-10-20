using System;
using System.Threading.Tasks;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Core.Quality.Services;

public class WpsWorkflowService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;

    public WpsWorkflowService(
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization)
    {
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;
    }

    public async Task ApproveAsync(Wps wps)
    {
        await EnsureApprovalPermissionAsync();

        if (wps == null)
            throw new ArgumentNullException(nameof(wps));

        if (wps.IsApproved)
        {
            throw new InvalidOperationException(
                "WPS is already approved.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Username))
        {
            throw new UnauthorizedAccessException(
                "Authenticated user does not have a valid username.");
        }

        wps.IsApproved = true;
        wps.IsLocked = true;
        wps.ApprovedBy = _currentUser.Username;
        wps.ApprovedOn = DateTime.UtcNow;
    }

    public async Task RejectAsync(
        Wps wps)
    {
        await EnsureApprovalPermissionAsync();

        if (wps == null)
            throw new ArgumentNullException(nameof(wps));

        wps.IsApproved = false;
        wps.ApprovedBy = null;
        wps.ApprovedOn = null;
    }

    private async Task EnsureApprovalPermissionAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(
            _currentUser.Role))
        {
            throw new UnauthorizedAccessException(
                "User does not have an assigned role.");
        }

        var hasPermission =
            await _permissionAuthorization
                .HasPermissionAsync(
                    _currentUser.UserId,
                    _currentUser.Role,
                    PermissionKeys.Quality.WpsApproval);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "User does not have permission to approve or reject WPS.");
        }
    }
}



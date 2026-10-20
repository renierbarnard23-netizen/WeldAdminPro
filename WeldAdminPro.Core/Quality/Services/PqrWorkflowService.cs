using System;
using System.Threading.Tasks;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Core.Quality.Services;

public class PqrWorkflowService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPermissionAuthorizationService _permissionAuthorization;

    public PqrWorkflowService(
        ICurrentUserContext currentUser,
        IPermissionAuthorizationService permissionAuthorization)
    {
        _currentUser = currentUser;
        _permissionAuthorization = permissionAuthorization;
    }

    public async Task ApproveAsync(Pqr pqr)
    {
        await EnsureApprovalPermissionAsync();

        if (pqr == null)
            throw new ArgumentNullException(nameof(pqr));

        if (pqr.IsApproved)
        {
            throw new InvalidOperationException(
                "PQR is already approved.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Username))
        {
            throw new UnauthorizedAccessException(
                "Authenticated user does not have a valid username.");
        }

        pqr.IsApproved = true;
        pqr.IsLocked = true;
        pqr.ApprovedBy = _currentUser.Username;
        pqr.ApprovedOn = DateTime.UtcNow;
    }

    private async Task EnsureApprovalPermissionAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Role))
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
                "User does not have permission to approve PQR.");
        }
    }
}

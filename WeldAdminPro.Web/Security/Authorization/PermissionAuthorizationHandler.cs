using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WeldAdminPro.Data.Services.Security;

namespace WeldAdminPro.Web.Security.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly PermissionAuthorizationService
        _permissionAuthorization;

    public PermissionAuthorizationHandler(
        PermissionAuthorizationService permissionAuthorization)
    {
        _permissionAuthorization =
            permissionAuthorization;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var roleName =
            context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(roleName))
            return;

        var allowed =
            await _permissionAuthorization.HasPermissionAsync(
                roleName,
                requirement.PermissionKey);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
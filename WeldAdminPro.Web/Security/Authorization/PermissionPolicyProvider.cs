using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WeldAdminPro.Web.Security.Authorization;

public sealed class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "Permission:";

    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        if (!policyName.StartsWith(
                PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var permissionKey =
            policyName[PolicyPrefix.Length..];

        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            return null;
        }

        var policy =
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(
                    new PermissionRequirement(permissionKey))
                .Build();

        return policy;
    }
}
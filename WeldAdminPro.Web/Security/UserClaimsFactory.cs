using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security;

namespace WeldAdminPro.Web.Security;

public static class UserClaimsFactory
{
    public static ClaimsPrincipal Create(SystemUser user)
    {
        var roleName =
            SystemRoleMapper.ToDatabaseRole(user.Role);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.GivenName,
                    user.FullName),

                new Claim(
                    ClaimTypes.Role,
                    roleName)
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}
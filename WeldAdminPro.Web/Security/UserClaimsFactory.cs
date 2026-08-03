using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Web.Security;

public static class UserClaimsFactory
{
    public static ClaimsPrincipal Create(
        SystemUser user)
    {
        if (string.IsNullOrWhiteSpace(user.RoleName))
        {
            throw new InvalidOperationException(
                $"User '{user.Username}' does not have a valid database role.");
        }

        var identity =
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        user.Username),

                    new Claim(
                        ClaimTypes.GivenName,
                        user.FullName),

                    new Claim(
                        ClaimTypes.Role,
                        user.RoleName)
                },
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}

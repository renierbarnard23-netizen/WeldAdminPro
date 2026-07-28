using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Web.Security;

public class WeldAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    private ClaimsPrincipal _currentUser = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(
            new AuthenticationState(_currentUser));
    }

    public void SignIn(SystemUser user)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            },
            authenticationType: "WeldAdmin");

        _currentUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _currentUser = Anonymous;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }
}
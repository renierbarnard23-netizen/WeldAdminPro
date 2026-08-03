using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Web.Models;
using WeldAdminPro.Web.Security;

using UserAuthenticationService = WeldAdminPro.Data.Services.AuthenticationService;

namespace WeldAdminPro.Web.Controllers;

[Route("account")]
public class AccountController : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromServices] UserAuthenticationService authenticationService,
        [FromForm] LoginRequest request)
    {
        var user = authenticationService.Authenticate(
            request.Username,
            request.Password);

        if (user == null)
        {
            return Redirect("/login?error=1");
        }

        var principal = UserClaimsFactory.Create(user);

        foreach (var claim in principal.Claims)
        {
            Console.WriteLine($"{claim.Type} = {claim.Value}");
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Redirect("/");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/login");
    }
}
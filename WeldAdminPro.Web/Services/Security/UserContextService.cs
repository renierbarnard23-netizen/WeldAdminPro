using System.Security.Claims;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Web.Services.Security;

public class UserContextService : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string Username =>
        User?.Identity?.Name ?? "";

    public string FullName =>
        User?.FindFirst(ClaimTypes.GivenName)?.Value ?? "";

    public string Role =>
        User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
    public bool IsAdministrator =>
        string.Equals(
            Role,
            "Administrator",
            StringComparison.OrdinalIgnoreCase);
}
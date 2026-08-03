using System.Security.Claims;

namespace WeldAdminPro.Web.Services.Security;

public class UserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string? CurrentUserName =>
        User?.Identity?.Name;

    public string? CurrentFullName =>
        User?.FindFirst(ClaimTypes.GivenName)?.Value;

    public string? CurrentRole =>
        User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAdministrator =>
        string.Equals(
            CurrentRole,
            "Administrator",
            StringComparison.OrdinalIgnoreCase);
}
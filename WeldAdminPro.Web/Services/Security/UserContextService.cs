using System.Security.Claims;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Web.Services.Security;

public class UserContextService : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal? _circuitUser;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _circuitUser ??
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string UserId =>
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    public string Username =>
        User?.Identity?.Name ?? "";

    public string FullName =>
        User?.FindFirst(ClaimTypes.GivenName)?.Value ?? "";

    public string Role =>
        User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

    public Guid? CompanyId
    {
        get
        {
            var value =
                User?.FindFirst("WeldAdminPro:CompanyId")?.Value;

            if (Guid.TryParse(value, out var companyId))
                return companyId;

            return null;
        }
    }

    public bool IsAdministrator =>
        string.Equals(
            Role,
            "Administrator",
            StringComparison.OrdinalIgnoreCase);

    public void SetCircuitUser(ClaimsPrincipal user)
    {
        _circuitUser = user;
    }
}

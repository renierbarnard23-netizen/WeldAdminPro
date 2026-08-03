namespace WeldAdminPro.Core.Security.Abstractions;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    string Username { get; }

    string FullName { get; }

    string Role { get; }
}
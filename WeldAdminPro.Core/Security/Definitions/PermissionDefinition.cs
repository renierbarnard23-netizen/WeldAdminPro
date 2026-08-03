namespace WeldAdminPro.Core.Security.Definitions;

public sealed record PermissionDefinition(
    string Key,
    string Group,
    string Name,
    string Description);
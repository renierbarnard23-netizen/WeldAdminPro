namespace WeldAdminPro.Core.Security.Definitions;

public sealed record RoleDefinition(
    string Name,
    string Description,
    bool IsSystemRole);
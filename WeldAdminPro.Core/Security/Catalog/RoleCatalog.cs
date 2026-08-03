using WeldAdminPro.Core.Security.Definitions;

namespace WeldAdminPro.Core.Security.Catalog;

public static class RoleCatalog
{
    public static IReadOnlyList<RoleDefinition> All =>
    [
        new(
            "Administrator",
            "Full system access",
            true),

        new(
            "Production Manager",
            "Production planning and execution",
            true),

        new(
            "Quality Manager",
            "Quality management",
            true),

        new(
            "Project Manager",
            "Project management",
            true),

        new(
            "Store Controller",
            "Inventory management",
            true),

        new(
            "Engineer",
            "Engineering functions",
            true),

        new(
            "Supervisor",
            "Production supervision",
            true),

        new(
            "Viewer",
            "Read-only access",
            true)
    ];
}
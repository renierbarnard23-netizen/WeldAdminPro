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
            "Operations Manager",
            "Operations management, production, projects and reporting",
            true),

        new(
            "Quality Manager",
            "Quality management and compliance",
            true),

        new(
            "Welding Coordinator",
            "Welding coordination, WPS, PQR and weld quality management",
            true),

        new(
            "Production Supervisor",
            "Production supervision and work order execution",
            true),

        new(
            "QA Inspector",
            "Quality assurance and compliance",
            true),

        new(
            "QC Inspector",
            "Quality control, inspection and weld register functions",
            true),

        new(
            "Store Controller",
            "Inventory control, stock movement and goods receiving",
            true),

        new(
            "Welder",
            "Production execution and welding-related access",
            true),

        new(
            "Viewer",
            "Read-only system access",
            true)
    ];
}
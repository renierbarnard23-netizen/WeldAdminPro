using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Security;

public static class SystemRoleMapper
{
    public static string ToDatabaseRole(SystemRole role)
    {
        return role switch
        {
            SystemRole.Admin => "Administrator",

            SystemRole.Viewer => "Viewer",

            SystemRole.Welder => "Welder",

            SystemRole.QC => "QC Inspector",

            SystemRole.QA => "QA Inspector",

            SystemRole.Supervisor => "Production Supervisor",

            SystemRole.WeldingCoordinator => "Welding Coordinator",

            SystemRole.QualityManager => "Quality Manager",

            SystemRole.OperationsManager => "Operations Manager",

            SystemRole.StoreController => "Store Controller",

            _ => "Viewer"
        };
    }
}
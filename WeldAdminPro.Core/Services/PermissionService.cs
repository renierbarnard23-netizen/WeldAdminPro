using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Services
{
    public static class PermissionService
    {
        public static bool HasPermission(
            SystemPermission permission)
        {
            var role =
                CurrentUserContext.Role;

            if (!RolePermissionMatrix
                .Permissions
                .ContainsKey(role))
            {
                return false;
            }

            return RolePermissionMatrix
                .Permissions[role]
                .Contains(permission);
        }

        // =====================================
        // LEGACY SUPPORT
        // =====================================

        public static bool CanAccessQuality =>
            HasPermission(
                SystemPermission.AccessQuality);

        public static bool CanAccessProduction =>
            HasPermission(
                SystemPermission.AccessProduction);

        public static bool CanAccessReports =>
            HasPermission(
                SystemPermission.AccessReports);

        public static bool CanManageUsers =>
            HasPermission(
                SystemPermission.ManageUsers);

        public static bool CanReleaseWeld =>
            HasPermission(
                SystemPermission.ReleaseWeld);
    }
}